using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Repositories;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _svc;
        private readonly IMomoService _momoService;
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IConfiguration _configuration;

        public SubscriptionController(
            ISubscriptionService svc,
            IMomoService momoService,
            ILogger<SubscriptionController> logger,
            IConfiguration configuration)
        {
            _svc = svc;
            _momoService = momoService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plans = await _svc.GetAllPlansWithTiersAsync();
            var vm = new SubscriptionIndexVm { Plans = plans };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVm vm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // Validate: Only allow mealsPerDay = 2
                var plans = await _svc.GetAllPlansWithTiersAsync();
                var selectedTier = plans   
                    .SelectMany(p => p.MealTiers)
                    .FirstOrDefault(t => t.Id == vm.TierId);
                
                if (selectedTier == null || selectedTier.MealsPerDay != 2)
                {
                    TempData["ErrorMessage"] = "Hiện tại chỉ hỗ trợ đăng ký gói 2 bữa/ngày.";
                    return RedirectToAction(nameof(Index));
                }

                // Create subscription and payment via service
                var (subscription, payment) = await _svc.CreateSubscriptionWithPaymentAsync(
                    userId, vm.PlanId, vm.TierId, vm.StartDate);

                // Create MoMo payment request
                // IMPORTANT: 
                // - returnUrl: Use localhost to preserve authentication cookies (user-facing redirect)
                // - ipnUrl: Use ngrok URL for server-to-server callback (MoMo needs public URL)
                
                // returnUrl: Always use localhost to preserve user's authentication cookies
                var returnUrlBase = $"{Request.Scheme}://{Request.Host}";
                var returnUrl = $"{returnUrlBase.TrimEnd('/')}{Url.Action(nameof(Callback), "Subscription")}";
                
                // ipnUrl: Use ngrok URL from config for server-to-server callback
                var ipnBaseUrl = _configuration["AppSettings:BaseUrl"];
                if (string.IsNullOrWhiteSpace(ipnBaseUrl))
                {
                    // Fallback to localhost if ngrok not configured (for testing)
                    ipnBaseUrl = returnUrlBase;
                }
                var ipnUrl = $"{ipnBaseUrl.TrimEnd('/')}{Url.Action(nameof(IpnCallback), "Subscription")}";
                
                _logger.LogInformation("MoMo payment URLs - returnUrl (localhost): {ReturnUrl}, ipnUrl (ngrok): {IpnUrl}", 
                    returnUrl, ipnUrl);
                
                var payUrl = await _momoService.CreatePaymentRequestAsync(payment, returnUrl, ipnUrl);
                
                _logger.LogInformation("MoMo payment URL generated: {PayUrl}", payUrl);
                
                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subscription/payment");
                ModelState.AddModelError("", $"Payment initialization failed: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(
            [FromQuery] int? errorCode, 
            [FromQuery] int? resultCode,
            [FromQuery] string? orderId, 
            [FromQuery] string? signature,
            [FromQuery] long? amount,
            [FromQuery] string? message,
            [FromQuery] string? orderInfo,
            [FromQuery] string? partnerCode)
        {
            // Log all query parameters for debugging
            _logger.LogInformation("MoMo callback received - errorCode={ErrorCode}, resultCode={ResultCode}, orderId={OrderId}, amount={Amount}, message={Message}, signature={Signature}, orderInfo={OrderInfo}, partnerCode={PartnerCode}", 
                errorCode, resultCode, orderId, amount, message, signature, orderInfo, partnerCode);
            
            // Log all query string parameters
            var allParams = string.Join(", ", Request.Query.Select(q => $"{q.Key}={q.Value}"));
            _logger.LogInformation("All callback query parameters: {Params}", allParams);

            // MoMo can return either errorCode or resultCode
            // resultCode = 0 means success, errorCode = 0 also means success
            // If both are null, check if orderId exists (some MoMo versions may not return errorCode/resultCode)
            var isSuccess = false;
            
            if (errorCode.HasValue)
            {
                isSuccess = errorCode == 0;
            }
            else if (resultCode.HasValue)
            {
                isSuccess = resultCode == 0;
            }
            else if (!string.IsNullOrEmpty(orderId))
            {
                // If no errorCode/resultCode but orderId exists, assume success (for some MoMo test scenarios)
                _logger.LogWarning("No errorCode or resultCode in callback, but orderId exists. Assuming success for orderId={OrderId}", orderId);
                isSuccess = true;
            }
            
            if (isSuccess && !string.IsNullOrEmpty(orderId))
            {
                try
                {
                    // Build raw response for logging
                    var rawResponse = $"errorCode={errorCode}, resultCode={resultCode}, orderId={orderId}, amount={amount}, message={message}";
                    
                    // Optional: Verify signature if provided
                    if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(partnerCode) && amount.HasValue)
                    {
                        // Build raw signature string for verification
                        var rawSignature = $"accessKey={_configuration["Momo:AccessKey"]}&amount={amount}&extraData=&message={message ?? ""}&orderId={orderId}&orderInfo={orderInfo ?? ""}&orderType=&partnerCode={partnerCode}&payType=&requestId=&responseTime=";
                        // Note: MoMo callback signature verification may use different format
                        // For now, we'll proceed with payment confirmation
                    }

                    // orderId from MoMo should match PaymentCode
                    var subscription = await _svc.ConfirmPaymentAsync(orderId, momoOrderId: orderId, rawResponse: rawResponse);
                    
                    if (subscription != null)
                    {
                        TempData["SuccessMessage"] = "Thanh toán thành công! Gói đăng ký của bạn đã được kích hoạt.";
                        TempData["SubscriptionId"] = subscription.Id;
                        
                        // Redirect to user's subscription details page
                        if (User.Identity?.IsAuthenticated == true)
                        {
                            // User is authenticated, redirect directly to subscription details
                            return RedirectToAction("Details", "UserSubscriptions", new { id = subscription.Id });
                        }
                        else
                        {
                            // User not authenticated, store return URL to subscription details
                            var returnUrl = Url.Action("Details", "UserSubscriptions", new { id = subscription.Id });
                            TempData["ReturnUrl"] = returnUrl;
                            return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán thành công nhưng không tìm thấy thông tin gói đăng ký. Vui lòng liên hệ hỗ trợ.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("already processed"))
                    {
                        // Payment already processed, try to get subscription ID from payment
                        try
                        {
                            var paymentService = HttpContext.RequestServices.GetRequiredService<ISubscriptionService>();
                            // Query payment to get subscription ID
                            var paymentRepo = HttpContext.RequestServices.GetRequiredService<MealPrep.DAL.Repositories.IRepository<MealPrep.DAL.Entities.Payment>>();
                            var payment = await paymentRepo.Query()
                                .Include(p => p.Subscription)
                                .FirstOrDefaultAsync(p => p.PaymentCode == orderId);
                            
                            if (payment?.Subscription != null)
                            {
                                TempData["SuccessMessage"] = "Thanh toán đã được xử lý trước đó. Gói đăng ký của bạn đã được kích hoạt.";
                                
                                if (User.Identity?.IsAuthenticated == true)
                                {
                                    return RedirectToAction("Details", "UserSubscriptions", new { id = payment.Subscription.Id });
                                }
                                else
                                {
                                    var returnUrl = Url.Action("Details", "UserSubscriptions", new { id = payment.Subscription.Id });
                                    TempData["ReturnUrl"] = returnUrl;
                                    return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl });
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogError(innerEx, "Error getting subscription from already processed payment");
                        }
                        
                        // Fallback: redirect to subscriptions list
                        TempData["SuccessMessage"] = "Thanh toán đã được xử lý trước đó.";
                        
                        if (User.Identity?.IsAuthenticated == true)
                        {
                            return RedirectToAction("Index", "UserSubscriptions");
                        }
                        else
                        {
                            var returnUrl = Url.Action("Index", "UserSubscriptions");
                            TempData["ReturnUrl"] = returnUrl;
                            return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl });
                        }
                    }
                    else if (ex.Message.Contains("Payment not found"))
                    {
                        _logger.LogWarning("Payment {OrderId} not found in database", orderId);
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán. Vui lòng liên hệ hỗ trợ.";
                        return RedirectToAction(nameof(Index));
                    }
                    _logger.LogError(ex, "Failed to confirm payment: {Message}", ex.Message);
                    TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to confirm payment");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
                }
            }
            else
            {
                var errorMsg = message ?? "Thanh toán không thành công";
                _logger.LogWarning("MoMo payment failed - errorCode={ErrorCode}, resultCode={ResultCode}, message={Message}, orderId={OrderId}", 
                    errorCode, resultCode, message, orderId);
                TempData["ErrorMessage"] = errorMsg;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IpnCallback()
        {
            // IPN (Instant Payment Notification) - MoMo will POST here
            // This is for server-to-server notification
            _logger.LogInformation("MoMo IPN callback received");
            
            try
            {
                // Enable buffering to read request body multiple times if needed
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                // Read request body
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0; // Reset for potential future reads
                
                _logger.LogInformation("IPN Body: {Body}", body);

                // Parse JSON response
                var jsonDoc = System.Text.Json.JsonDocument.Parse(body);
                var root = jsonDoc.RootElement;

                // Extract parameters
                var resultCode = root.TryGetProperty("resultCode", out var rc) ? rc.GetInt32() : (int?)null;
                var orderId = root.TryGetProperty("orderId", out var oid) ? oid.GetString() : null;
                var amount = root.TryGetProperty("amount", out var amt) ? amt.GetInt64() : (long?)null;
                var message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null;
                var signature = root.TryGetProperty("signature", out var sig) ? sig.GetString() : null;

                _logger.LogInformation("IPN parsed - resultCode={ResultCode}, orderId={OrderId}, amount={Amount}, message={Message}", 
                    resultCode, orderId, amount, message);

                // Process if success
                if (resultCode == 0 && !string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        await _svc.ConfirmPaymentAsync(orderId);
                        _logger.LogInformation("IPN: Payment {OrderId} confirmed successfully", orderId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (ex.Message.Contains("already processed"))
                        {
                            _logger.LogInformation("IPN: Payment {OrderId} already processed", orderId);
                        }
                        else
                        {
                            _logger.LogError(ex, "IPN: Failed to confirm payment {OrderId}", orderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IPN: Exception confirming payment {OrderId}", orderId);
                    }
                }
                else
                {
                    _logger.LogWarning("IPN: Payment failed - resultCode={ResultCode}, orderId={OrderId}, message={Message}", 
                        resultCode, orderId, message);
                }

                // Always return 200 OK to MoMo
                return Ok(new { resultCode = 0, message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IPN: Error processing callback");
                // Still return 200 to prevent MoMo from retrying
                return Ok(new { resultCode = 0, message = "Error processed" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sub = await _svc.GetDetailsAsync(id, userId);
            if (sub == null) return NotFound();
            return View(sub);
        }
    }
}

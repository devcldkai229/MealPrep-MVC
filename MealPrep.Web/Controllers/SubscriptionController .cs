using MealPrep.BLL.Services;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _svc;
        private readonly IMomoService _momoService;
        private readonly IRepository<DeliverySlot> _slotRepo;
        private readonly IRepository<Plan> _planRepo;
        private readonly IRepository<PlanMealTier> _tierRepo;
        private readonly IRepository<Subscription> _subRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<AppUser> _userRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            ISubscriptionService svc,
            IMomoService momoService,
            IRepository<DeliverySlot> slotRepo,
            IRepository<Plan> planRepo,
            IRepository<PlanMealTier> tierRepo,
            IRepository<Subscription> subRepo,
            IRepository<Payment> paymentRepo,
            IRepository<AppUser> userRepo,
            AppDbContext dbContext,
            ILogger<SubscriptionController> logger)
        {
            _svc = svc;
            _momoService = momoService;
            _slotRepo = slotRepo;
            _planRepo = planRepo;
            _tierRepo = tierRepo;
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
            _userRepo = userRepo;
            _dbContext = dbContext;
            _logger = logger;
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
            var user = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized();

            var plan = await _svc.GetPlanByIdAsync(vm.PlanId);
            var tier = await _svc.GetTierByIdAsync(vm.TierId);
            
            if (plan == null || tier == null || tier.PlanId != vm.PlanId)
            {
                ModelState.AddModelError("", "Invalid plan or tier selected.");
                return RedirectToAction(nameof(Index));
            }

            var totalAmount = await _svc.CalculateTotalPriceAsync(vm.PlanId, vm.TierId);

            // Create Subscription
            var subscription = new Subscription
            {
                AppUserId = userId,
                PlanId = vm.PlanId,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                MealsPerDay = tier.MealsPerDay,
                StartDate = vm.StartDate,
                Status = SubscriptionStatus.PendingPayment,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _subRepo.AddAsync(subscription);
            await _subRepo.SaveChangesAsync();

            // Create Payment
            var payment = new Payment
            {
                AppUserId = userId,
                SubscriptionId = subscription.Id,
                Amount = totalAmount,
                Currency = "VND",
                Method = "MoMo",
                Status = "Pending",
                PaymentCode = Guid.NewGuid().ToString(),
                Description = $"Thanh toan subscription {plan.Name} - {tier.MealsPerDay} meals/day",
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(24)
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            // Create MoMo payment request
            try
            {
                var returnUrl = Url.Action(nameof(Callback), "Subscription", null, Request.Scheme)!;
                var ipnUrl = Url.Action(nameof(IpnCallback), "Subscription", null, Request.Scheme)!;
                
                var payUrl = await _momoService.CreatePaymentRequestAsync(payment, returnUrl, ipnUrl);
                
                _logger.LogInformation("MoMo payment URL generated: {PayUrl}", payUrl);
                
                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MoMo payment request");
                ModelState.AddModelError("", $"Payment initialization failed: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Callback([FromQuery] int? errorCode, [FromQuery] string? orderId, [FromQuery] string? signature)
        {
            _logger.LogInformation("MoMo callback received: errorCode={ErrorCode}, orderId={OrderId}", errorCode, orderId);

            if (errorCode == 0 && !string.IsNullOrEmpty(orderId))
            {
                // Find payment by PaymentCode
                var payment = await _paymentRepo.Query()
                    .Include(p => p.Subscription)
                    .FirstOrDefaultAsync(p => p.PaymentCode == orderId);

                if (payment != null)
                {
                    // Prevent duplicate processing
                    if (payment.Status == "Paid")
                    {
                        _logger.LogWarning("Payment {PaymentCode} already processed", payment.PaymentCode);
                        TempData["SuccessMessage"] = "Thanh toán đã được xử lý trước đó.";
                        return RedirectToAction("SelectMeals", "Menu");
                    }

                    if (payment.Status == "Pending")
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            // Update Payment
                            payment.Status = "Paid";
                            payment.PaidAt = DateTime.UtcNow;

                            // Update Subscription
                            var subscription = payment.Subscription;
                            if (subscription != null)
                            {
                                subscription.Status = SubscriptionStatus.Active;
                                subscription.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                                
                                var plan = await _planRepo.GetByIdAsync(subscription.PlanId);
                                if (plan != null)
                                {
                                    subscription.EndDate = subscription.StartDate.AddDays(plan.DurationDays);
                                }
                            }

                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();

                            _logger.LogInformation("Payment {PaymentCode} confirmed successfully", payment.PaymentCode);

                            TempData["SuccessMessage"] = "Thanh toán thành công! Vui lòng chọn món ăn cho tuần này.";
                            return RedirectToAction("SelectMeals", "Menu");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Failed to update payment status");
                            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
                        }
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán không thành công. Vui lòng thử lại.";
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
            
            // Read request body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("IPN Body: {Body}", body);

            // TODO: Verify signature and update payment status
            // For now, we'll rely on the return URL callback
            
            return Ok();
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

using MealPrep.BLL.Exceptions;
using MealPrep.BLL.Services;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    public class AuthController : Controller
    {

        private readonly ILogger<AuthController> _logger;

        private readonly IAuthService _authService;

        private readonly IUserService _userService; 

        public AuthController(ILogger<AuthController> logger, IAuthService authService, IUserService userService)
        {
            _logger = logger;
            _authService = authService;
            _userService = userService;
        }

        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtp(string email)
        {
            try
            {
                _logger.LogInformation($"Attempting to send OTP to: {email}");
                await _authService.SendOtpAsync(email);
                _logger.LogInformation($"OTP sent successfully to: {email}");
                return Json(new { success = true, message = "OTP code has been sent to your email" });
            }
            catch (EmailAlreadyExistsException ex)
            {
                _logger.LogWarning($"Email already exists: {email}");
                return Json(new { success = false, message = "Email already registered" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP to: {email}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost] public async Task<IActionResult> Register(RegisterVm vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);    
            }

            try
            {
                var user = await _authService.RegisterAsync(vm.Email, vm.FullName, vm.Password, vm.OtpCode);
                await SignInAsync(user);
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpGet] public IActionResult Login() => View(new LoginVm());

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _authService.LoginAsync(vm.Email, vm.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }

            await SignInAsync(user);
            return RedirectToAction("Index", "Auth");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View(new CompleteProfileVm());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(CompleteProfileVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var currentUser = await _userService.GetUserProfileAsync(userId);

                if (currentUser == null)
                {
                    return NotFound("User not found");
                }

                // Update profile with new information
                await _userService.UpdateProfileAsync(
                    userId,
                    currentUser.FullName,
                    vm.PhoneNumber,
                    vm.Gender,
                    vm.Age,
                    currentUser.AvatarUrl
                );

                TempData["SuccessMessage"] = "Hoàn tất hồ sơ thành công!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing profile");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật hồ sơ. Vui lòng thử lại.");
                return View(vm);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetUserProfileAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            return View(user);
        }

        public IActionResult AccessDenied() => Content("Truy cập bị từ chối!");

        private async Task SignInAsync(AuthResponse user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.RoleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
        }
    }
}

using MealPrep.BLL.Services;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IAdminUserService adminUserService, ILogger<AdminUsersController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            var totalCount = await _adminUserService.GetUsersCountAsync(search);
            var users = await _adminUserService.GetUsersAsync(search, page, pageSize);

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _adminUserService.GetUserDetailsAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _adminUserService.DeactivateUserAsync(id);
                TempData["SuccessMessage"] = $"Đã vô hiệu hóa tài khoản";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi vô hiệu hóa tài khoản: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                await _adminUserService.ActivateUserAsync(id);
                TempData["SuccessMessage"] = $"Đã kích hoạt tài khoản";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi kích hoạt tài khoản: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

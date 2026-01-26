using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPlansController : Controller
    {
        private readonly IAdminPlanService _adminPlanService;
        private readonly ILogger<AdminPlansController> _logger;

        public AdminPlansController(
            IAdminPlanService adminPlanService,
            ILogger<AdminPlansController> logger)
        {
            _adminPlanService = adminPlanService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plans = await _adminPlanService.GetAllPlansAsync();
            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var plan = await _adminPlanService.GetPlanDetailsAsync(id);
            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Plan());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plan plan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminPlanService.CreatePlanAsync(plan);
                    TempData["SuccessMessage"] = $"Đã tạo gói {plan.Name} thành công";
                    return RedirectToAction(nameof(Details), new { id = plan.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating plan");
                    ModelState.AddModelError("", $"Lỗi khi tạo gói: {ex.Message}");
                }
            }
            return View(plan);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _adminPlanService.GetPlanDetailsAsync(id);
            if (plan == null)
            {
                return NotFound();
            }
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plan plan)
        {
            if (id != plan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _adminPlanService.UpdatePlanAsync(id, plan);
                    TempData["SuccessMessage"] = $"Đã cập nhật gói {plan.Name} thành công";
                    return RedirectToAction(nameof(Details), new { id = plan.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating plan");
                    ModelState.AddModelError("", $"Lỗi khi cập nhật gói: {ex.Message}");
                }
            }
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await _adminPlanService.CanDeletePlanAsync(id))
                {
                    TempData["ErrorMessage"] = "Không thể xóa gói đang có subscription đang hoạt động";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _adminPlanService.DeletePlanAsync(id);
                TempData["SuccessMessage"] = "Đã xóa gói thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plan");
                TempData["ErrorMessage"] = $"Lỗi khi xóa gói: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

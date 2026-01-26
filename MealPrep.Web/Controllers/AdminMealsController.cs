using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMealsController : Controller
    {
        private readonly IMealService _mealService;
        private readonly ILogger<AdminMealsController> _logger;

        public AdminMealsController(
            IMealService mealService,
            ILogger<AdminMealsController> logger)
        {
            _mealService = mealService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1, int pageSize = 20)
        {
            var (meals, totalCount) = await _mealService.GetMealsForAdminAsync(search, isActive, page, pageSize);

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(meals);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var meal = await _mealService.GetAsync(id);
            if (meal == null)
            {
                return NotFound();
            }

            return View(meal);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Meal());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meal meal)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _mealService.CreateAsync(meal);
                    TempData["SuccessMessage"] = $"Đã tạo món ăn {meal.Name} thành công";
                    return RedirectToAction(nameof(Details), new { id = meal.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating meal");
                    ModelState.AddModelError("", $"Lỗi khi tạo món ăn: {ex.Message}");
                }
            }
            return View(meal);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var meal = await _mealService.GetAsync(id);
            if (meal == null)
            {
                return NotFound();
            }
            return View(meal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Meal meal)
        {
            if (id != meal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _mealService.UpdateAsync(meal);
                    TempData["SuccessMessage"] = $"Đã cập nhật món ăn {meal.Name} thành công";
                    return RedirectToAction(nameof(Details), new { id = meal.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating meal");
                    ModelState.AddModelError("", $"Lỗi khi cập nhật món ăn: {ex.Message}");
                }
            }
            return View(meal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var meal = await _mealService.GetAsync(id);
            if (meal == null)
            {
                return NotFound();
            }

            try
            {
                await _mealService.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Đã xóa món ăn {meal.Name}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meal");
                TempData["ErrorMessage"] = $"Lỗi khi xóa món ăn: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

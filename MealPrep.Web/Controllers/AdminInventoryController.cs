using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminInventoryController : Controller
    {
        private readonly IAdminInventoryService _inventoryService;
        private readonly IMealService _mealService;
        private readonly ILogger<AdminInventoryController> _logger;

        public AdminInventoryController(
            IAdminInventoryService inventoryService,
            IMealService mealService,
            ILogger<AdminInventoryController> logger)
        {
            _inventoryService = inventoryService;
            _mealService = mealService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? date = null, int page = 1, int pageSize = 20)
        {
            DateOnly targetDate;
            if (string.IsNullOrEmpty(date) || !DateOnly.TryParse(date, out targetDate))
            {
                targetDate = DateOnly.FromDateTime(DateTime.Today);
            }

            var inventories = await _inventoryService.GetInventoryByDateAsync(targetDate);
            
            // Calculate used quantities for each inventory
            foreach (var inv in inventories)
            {
                inv.QuantityUsed = await _inventoryService.GetUsedQuantityAsync(inv.MealId, inv.Date);
            }

            ViewBag.SelectedDate = targetDate;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(inventories);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? date = null)
        {
            DateOnly targetDate;
            if (string.IsNullOrEmpty(date) || !DateOnly.TryParse(date, out targetDate))
            {
                targetDate = DateOnly.FromDateTime(DateTime.Today);
            }

            var inventory = new KitchenInventory
            {
                Date = targetDate
            };

            await LoadMealsSelectList();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KitchenInventory inventory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if inventory already exists for this date and meal
                    var exists = await _inventoryService.ExistsForDateAndMealAsync(inventory.Date, inventory.MealId);

                    if (exists)
                    {
                        ModelState.AddModelError("MealId", "Đã có giới hạn kho cho món này vào ngày này. Vui lòng cập nhật thay vì tạo mới.");
                        await LoadMealsSelectList();
                        return View(inventory);
                    }

                    await _inventoryService.SaveInventoryAsync(inventory);
                    TempData["SuccessMessage"] = "Đã tạo giới hạn kho thành công.";
                    return RedirectToAction(nameof(Index), new { date = inventory.Date.ToString("yyyy-MM-dd") });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating inventory");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo giới hạn kho: " + ex.Message);
                }
            }

            await LoadMealsSelectList();
            return View(inventory);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            inventory.QuantityUsed = await _inventoryService.GetUsedQuantityAsync(inventory.MealId, inventory.Date);
            await LoadMealsSelectList();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KitchenInventory inventory)
        {
            if (id != inventory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if another inventory exists for this date and meal (different ID)
                    var exists = await _inventoryService.ExistsForDateAndMealAsync(inventory.Date, inventory.MealId, inventory.Id);

                    if (exists)
                    {
                        ModelState.AddModelError("MealId", "Đã có giới hạn kho khác cho món này vào ngày này.");
                        inventory.QuantityUsed = await _inventoryService.GetUsedQuantityAsync(inventory.MealId, inventory.Date);
                        await LoadMealsSelectList();
                        return View(inventory);
                    }

                    await _inventoryService.SaveInventoryAsync(inventory);
                    TempData["SuccessMessage"] = "Đã cập nhật giới hạn kho thành công.";
                    return RedirectToAction(nameof(Index), new { date = inventory.Date.ToString("yyyy-MM-dd") });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating inventory");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật giới hạn kho: " + ex.Message);
                }
            }

            inventory.QuantityUsed = await _inventoryService.GetUsedQuantityAsync(inventory.MealId, inventory.Date);
            await LoadMealsSelectList();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var date = inventory.Date;
            var deleted = await _inventoryService.DeleteInventoryAsync(id);
            
            if (deleted)
            {
                TempData["SuccessMessage"] = "Đã xóa giới hạn kho thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa giới hạn kho.";
            }

            return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
        }

        [HttpGet]
        public async Task<IActionResult> BulkCreate(string? startDate = null, string? endDate = null)
        {
            DateOnly start, end;
            if (string.IsNullOrEmpty(startDate) || !DateOnly.TryParse(startDate, out start))
            {
                start = DateOnly.FromDateTime(DateTime.Today);
            }
            if (string.IsNullOrEmpty(endDate) || !DateOnly.TryParse(endDate, out end))
            {
                end = start.AddDays(6); // Default 7 days
            }

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            
            // Load all active meals for checkbox selection
            var meals = await _mealService.GetAllMealsAsync();
            ViewBag.Meals = new SelectList(meals.Where(m => m.IsActive), "Id", "Name");
            
            return View(meals.Where(m => m.IsActive).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(string startDate, string endDate, int[] mealIds, int quantityLimit)
        {
            if (!DateOnly.TryParse(startDate, out var start) || !DateOnly.TryParse(endDate, out var end))
            {
                ModelState.AddModelError("", "Ngày không hợp lệ.");
                await LoadMealsSelectList();
                return View();
            }

            if (start > end)
            {
                ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
                await LoadMealsSelectList();
                return View();
            }

            if (mealIds == null || mealIds.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một món ăn.");
                var meals = await _mealService.GetAllMealsAsync();
                ViewBag.Meals = new SelectList(meals.Where(m => m.IsActive), "Id", "Name");
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                return View(meals.Where(m => m.IsActive).ToList());
            }

            try
            {
                var (created, updated, totalMeals, totalDays) =
                    await _inventoryService.BulkCreateInventoriesAsync(start, end, mealIds, quantityLimit);

                TempData["SuccessMessage"] = $"Đã tạo {created} và cập nhật {updated} giới hạn kho thành công cho {totalMeals} món ăn trong {totalDays} ngày.";
                return RedirectToAction(nameof(Index), new { date = start.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating inventory");
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                var meals = await _mealService.GetAllMealsAsync();
                ViewBag.Meals = new SelectList(meals.Where(m => m.IsActive), "Id", "Name");
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                return View(meals.Where(m => m.IsActive).ToList());
            }
        }

        private async Task LoadMealsSelectList()
        {
            var meals = await _mealService.GetAllMealsAsync();
            ViewBag.Meals = new SelectList(meals.Where(m => m.IsActive), "Id", "Name");
        }
    }
}

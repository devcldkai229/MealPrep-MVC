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
        private readonly IS3Service _s3Service;
        private readonly ILogger<AdminMealsController> _logger;

        public AdminMealsController(
            IMealService mealService,
            IS3Service s3Service,
            ILogger<AdminMealsController> logger)
        {
            _mealService = mealService;
            _s3Service = s3Service;
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
        public async Task<IActionResult> Create(Meal meal, List<IFormFile>? imageFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload images to S3 if provided
                    var imageUrls = new List<string>();
                    if (imageFiles != null && imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0 && file.ContentType.StartsWith("image/"))
                            {
                                try
                                {
                                    var s3Key = await _s3Service.UploadFileAsync(
                                        file.OpenReadStream(),
                                        file.FileName,
                                        "meals",
                                        file.ContentType);
                                    
                                    // Get presigned URL for the uploaded image
                                    var imageUrl = _s3Service.GetPresignedUrl(s3Key, 8760); // 1 year expiration
                                    imageUrls.Add(imageUrl);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error uploading image {FileName} to S3", file.FileName);
                                    ModelState.AddModelError("", $"Lỗi khi upload ảnh {file.FileName}: {ex.Message}");
                                }
                            }
                        }
                    }

                    // If images were uploaded, update meal.Images
                    if (imageUrls.Any())
                    {
                        meal.Images = System.Text.Json.JsonSerializer.Serialize(imageUrls);
                    }

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
        public async Task<IActionResult> Edit(int id, Meal meal, List<IFormFile>? imageFiles)
        {
            if (id != meal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing meal to preserve existing images
                    var existingMeal = await _mealService.GetAsync(id);
                    var existingImageUrls = new List<string>();
                    
                    if (existingMeal != null && !string.IsNullOrWhiteSpace(existingMeal.Images))
                    {
                        try
                        {
                            existingImageUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(existingMeal.Images) ?? new List<string>();
                        }
                        catch
                        {
                            // If parsing fails, keep empty list
                        }
                    }

                    // Upload new images to S3 if provided
                    if (imageFiles != null && imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0 && file.ContentType.StartsWith("image/"))
                            {
                                try
                                {
                                    var s3Key = await _s3Service.UploadFileAsync(
                                        file.OpenReadStream(),
                                        file.FileName,
                                        "meals",
                                        file.ContentType);
                                    
                                    // Get presigned URL for the uploaded image
                                    var imageUrl = _s3Service.GetPresignedUrl(s3Key, 8760); // 1 year expiration
                                    existingImageUrls.Add(imageUrl);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error uploading image {FileName} to S3", file.FileName);
                                    ModelState.AddModelError("", $"Lỗi khi upload ảnh {file.FileName}: {ex.Message}");
                                }
                            }
                        }
                    }

                    // Update meal.Images with all images (existing + new)
                    if (existingImageUrls.Any())
                    {
                        meal.Images = System.Text.Json.JsonSerializer.Serialize(existingImageUrls);
                    }

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

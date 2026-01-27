using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMealsController : Controller
    {
        private readonly IMealService _mealService;
        private readonly IS3Service _s3Service;
        private readonly IMealFeedbackService _mealFeedbackService;
        private readonly ILogger<AdminMealsController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AdminMealsController(
            IMealService mealService,
            IS3Service s3Service,
            IMealFeedbackService mealFeedbackService,
            ILogger<AdminMealsController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _mealService = mealService;
            _s3Service = s3Service;
            _mealFeedbackService = mealFeedbackService;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
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

            // Load ratings for this meal via BLL service
            var ratings = await _mealFeedbackService.GetMealRatingsAsync(id);
            ViewBag.MealRatings = ratings;
            ViewBag.MealRatingCount = ratings.Count;
            ViewBag.MealRatingAverage = ratings.Count > 0
                ? Math.Round(ratings.Average(r => r.Stars), 1)
                : 0;

            return View(meal);
        }

        /// <summary>
        /// Gọi Python service (generate_meal_ai) để phân tích audio và trả về thông tin món ăn gợi ý.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeMealVoice(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn file audio." });
            }

            try
            {
                var baseUrl = _configuration["GenerateMealAi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return StatusCode(500, new { success = false, message = "Chưa cấu hình URL cho dịch vụ GenerateMeal AI (GenerateMealAi:BaseUrl)." });
                }

                var requestUrl = $"{baseUrl.TrimEnd('/')}/api/analyze-voice";

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(audioFile.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);
                content.Add(streamContent, "file", audioFile.FileName);

                var response = await _httpClient.PostAsync(requestUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AnalyzeMealVoice error from Python service: {Status} - {Body}", response.StatusCode, errorText);
                    return StatusCode(500, new { success = false, message = "AI không phân tích được audio. Vui lòng thử lại hoặc nhập tay." });
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                var dishName = root.GetProperty("dish_name").GetString() ?? string.Empty;
                var description = root.GetProperty("description").GetString() ?? string.Empty;

                // Map ingredients list => JSON array of string cho field Ingredients
                // Ví dụ: ["150g Ức gà (240 kcal)", "50g Rau xà lách (10 kcal)"]
                var ingredientsArray = new List<string>();
                if (root.TryGetProperty("ingredients", out var ingredientsEl) && ingredientsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var ing in ingredientsEl.EnumerateArray())
                    {
                        var name = ing.GetProperty("name").GetString() ?? "";
                        var amount = ing.GetProperty("amount_value").GetDouble();
                        var unit = ing.GetProperty("unit").GetString() ?? "";
                        var calories = ing.GetProperty("calories").GetInt32();
                        ingredientsArray.Add($"{amount}{unit} {name} ({calories} kcal)");
                    }
                }

                var ingredientsJson = System.Text.Json.JsonSerializer.Serialize(ingredientsArray);

                var totalNutrition = root.GetProperty("total_nutrition");
                var totalCalories = totalNutrition.GetProperty("calories").GetInt32();
                var protein = (decimal)totalNutrition.GetProperty("protein_g").GetDouble();
                var carbs = (decimal)totalNutrition.GetProperty("carbs_g").GetDouble();
                var fat = (decimal)totalNutrition.GetProperty("fat_g").GetDouble();

                decimal basePrice = 0;
                if (root.TryGetProperty("base_price_estimate", out var priceEl) && priceEl.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    basePrice = (decimal)priceEl.GetDouble();
                }

                return Json(new
                {
                    success = true,
                    dishName,
                    description,
                    ingredientsJson,
                    calories = totalCalories,
                    protein,
                    carbs,
                    fat,
                    basePrice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AnalyzeMealVoice");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi gọi AI. Vui lòng thử lại sau." });
            }
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

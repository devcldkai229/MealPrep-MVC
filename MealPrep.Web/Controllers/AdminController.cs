using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMealService _mealService;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IMealService mealService,
            IAdminDashboardService dashboardService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _mealService = mealService;
            _dashboardService = dashboardService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardVm
            {
                TodayRevenue = await _dashboardService.GetTodayRevenueAsync(),
                ActiveSubscribers = await _dashboardService.GetActiveSubscribersCountAsync(),
                TomorrowOrders = await _dashboardService.GetTomorrowOrdersCountAsync(),
                KitchenPrepForecast = await _dashboardService.GetKitchenPrepForecastAsync(),
                RevenueGrowth = await _dashboardService.GetRevenueGrowthAsync(30),
                MonthlyRevenue = await _dashboardService.GetMonthlyRevenueAsync(),
                SubscriptionGrowth = await _dashboardService.GetSubscriptionGrowthAsync(30),
                UserSegmentationByGoal = await _dashboardService.GetUserSegmentationByGoalAsync(),
                TopDislikedMeals = await _dashboardService.GetTopDislikedMealsAsync(5),
                OrdersStatusDistribution = await _dashboardService.GetOrdersStatusDistributionAsync(),
                AtRiskSubscriptions = await _dashboardService.GetAtRiskSubscriptionsAsync(3),
                FailedPayments = await _dashboardService.GetFailedPaymentsAsync(10),
                TopAllergies = await _dashboardService.GetTopAllergiesAsync()
            };
            
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var allMeals = await _mealService.GetAllActiveMealsAsync();
                var totalMeals = allMeals.Count;
                var mealsWithEmbedding = allMeals.Count(m => !string.IsNullOrEmpty(m.EmbeddingJson));
                var mealsWithoutEmbedding = totalMeals - mealsWithEmbedding;

                return Json(new
                {
                    totalMeals,
                    mealsWithEmbedding,
                    mealsWithoutEmbedding
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return Json(new { totalMeals = 0, mealsWithEmbedding = 0, mealsWithoutEmbedding = 0 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAllMealEmbeddings()
        {
            try
            {
                // Sử dụng batch endpoint mới - nhanh hơn và hiệu quả hơn
                var pythonApiUrl = _configuration["AiSettings:ServiceUrl"]?.Replace("/api/generate-menu", "/api/generate-all-meal-embeddings")
                    ?? "http://localhost:8000/api/generate-all-meal-embeddings";

                _logger.LogInformation("Calling Python batch embedding API: {Url}", pythonApiUrl);

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(15); // Timeout dài hơn cho batch processing

                // Gửi POST request không có body - Python sẽ tự query tất cả meals từ database
                var response = await httpClient.PostAsync(pythonApiUrl, null);

                _logger.LogInformation("Python batch API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BatchEmbeddingResponse>();
                    if (result != null && result.success)
                    {
                        _logger.LogInformation("✓ Batch embedding completed: {SuccessCount} success, {FailCount} failed out of {Total} total", 
                            result.success_count, result.fail_count, result.total);

                        return Json(new
                        {
                            success = true,
                            message = result.message,
                            successCount = result.success_count,
                            failCount = result.fail_count,
                            total = result.total,
                            errors = result.results?.Where(r => !r.success).Take(10).Select(r => $"Meal {r.meal_id}: {r.error}").ToList() ?? new List<string>()
                        });
                    }
                    else
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Invalid batch response. Response: {Response}", responseText);
                        return Json(new { success = false, message = "Response không hợp lệ từ Python API" });
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Python batch API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return Json(new { success = false, message = $"Lỗi từ Python API: {response.StatusCode} - {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateAllMealEmbeddings");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSingleMealEmbedding(int mealId)
        {
            try
            {
                var meal = await _mealService.GetAsync(mealId);
                if (meal == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy món ăn" });
                }

                var pythonApiUrl = _configuration["AiSettings:ServiceUrl"]?.Replace("/api/generate-menu", "/api/generate-meal-embedding")
                    ?? "http://localhost:8000/api/generate-meal-embedding";

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                var request = new
                {
                    meal_id = meal.Id,
                    name = meal.Name,
                    ingredients = meal.Ingredients ?? "[]",
                    description = meal.Description ?? "",
                    calories = meal.Calories,
                    protein = (double)meal.Protein,
                    carbs = (double)meal.Carbs,
                    fat = (double)meal.Fat
                };

                // Log request để debug
                _logger.LogInformation("Calling Python API (single): {Url} with meal_id: {MealId}, name: {Name}", 
                    pythonApiUrl, meal.Id, meal.Name);

                var response = await httpClient.PostAsJsonAsync(pythonApiUrl, request);
                
                _logger.LogInformation("Python API response status: {StatusCode} for meal {MealId}", 
                    response.StatusCode, meal.Id);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.embedding_json))
                    {
                        meal.EmbeddingJson = result.embedding_json;
                        await _mealService.UpdateAsync(meal);
                        _logger.LogInformation("✓ Generated embedding for meal {MealId}: {MealName} (dimension: {Dimension})", 
                            meal.Id, meal.Name, result.dimension);
                        return Json(new { success = true, message = $"Đã generate embedding thành công cho món '{meal.Name}' (dimension: {result.dimension})" });
                    }
                    else
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Empty or invalid response for meal {MealId}. Response: {Response}", 
                            meal.Id, responseText);
                        return Json(new { success = false, message = "API trả về response rỗng" });
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Python API error for meal {MealId}: {StatusCode} - {Error}", 
                        mealId, response.StatusCode, errorContent);
                    return Json(new { success = false, message = $"Lỗi từ API: {response.StatusCode} - {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for meal {MealId}", mealId);
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private class EmbeddingResponse
        {
            public int meal_id { get; set; }
            public string embedding_json { get; set; } = string.Empty;
            public int dimension { get; set; }
        }

        private class BatchEmbeddingResponse
        {
            public bool success { get; set; }
            public string message { get; set; } = string.Empty;
            public int total { get; set; }
            public int success_count { get; set; }
            public int fail_count { get; set; }
            public List<BatchEmbeddingResult>? results { get; set; }
        }

        private class BatchEmbeddingResult
        {
            public int meal_id { get; set; }
            public bool success { get; set; }
            public int? dimension { get; set; }
            public string? error { get; set; }
        }
    }
}

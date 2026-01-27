using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;
using System.Text;

namespace MealPrep.BLL.Services
{
    public interface IAiMenuService
    {
        /// <summary>
        /// Generate AI menu recommendations without saving (for user review)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="remainingDates">List of dates that need meal planning (only future dates without orders)</param>
        /// <param name="weeklyNotes">Optional notes for this week. If provided, will override profile.Notes</param>
        Task<List<AiMenuPlanItem>> GenerateMenuAsync(Guid userId, List<DateOnly> remainingDates, string? weeklyNotes = null);
        
        /// <summary>
        /// Generate and save menu directly to database
        /// </summary>
        Task<int> GenerateAndSaveMenuAsync(Guid userId, DateOnly weekStart);
    }

    public class AiMenuService : IAiMenuService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _aiServiceUrl;

        public AiMenuService(AppDbContext context, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;

            // Lấy URL từ appsettings.json
            // Local: "http://localhost:8000/api/generate-menu"
            // Lambda: "https://abcdefgh.lambda-url.us-east-1.on.aws/api/generate-menu"
            _aiServiceUrl = config["AiSettings:ServiceUrl"]
                            ?? throw new Exception("AI Service URL not configured");
        }

        /// <summary>
        /// Generate AI menu recommendations without saving (for user review)
        /// Only generates for remaining dates (future dates without confirmed orders)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="remainingDates">List of dates that need meal planning (only future dates without orders)</param>
        /// <param name="weeklyNotes">Optional notes for this week. If provided, will override profile.Notes</param>
        public async Task<List<AiMenuPlanItem>> GenerateMenuAsync(Guid userId, List<DateOnly> remainingDates, string? weeklyNotes = null)
        {
            // 1. Lấy dữ liệu User với đầy đủ thông tin
            var user = await _context.Users
                .Include(u => u.NutritionProfile!)
                    .ThenInclude(np => np.Allergies)
                .Include(u => u.DislikedMeals)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.NutritionProfile == null) 
                throw new Exception("User profile missing");

            var profile = user.NutritionProfile;

            // 2. Tính calories nếu chưa có
            var caloriesInDay = profile.CaloriesInDay ?? CalculateTDEE(profile, user);

            // 3. Lấy danh sách allergies
            var allergies = profile.Allergies.Select(a => a.AllergyName).ToList();

            // 4. Xác định notes: ưu tiên weeklyNotes nếu có, nếu không thì dùng profile.Notes
            var notesToUse = !string.IsNullOrWhiteSpace(weeklyNotes) 
                ? weeklyNotes.Trim() 
                : (profile.Notes ?? "");

            // 5. Chuẩn bị Payload theo format Python API yêu cầu
            // Pass number of remaining days instead of always 7
            var numberOfDays = remainingDates.Count;
            
            var payload = new
            {
                user_profile = new
                {
                    height_cm = profile.HeightCm,
                    weight_kg = (double)profile.WeightKg,
                    goal = (int)profile.Goal,
                    activity_level = (int)profile.ActivityLevel,
                    meals_per_day = profile.MealsPerDay,
                    notes = notesToUse,
                    diet_pref = (int)profile.DietPreference,
                    calories_in_day = caloriesInDay,
                    allergies = allergies
                },
                disliked_ids = user.DislikedMeals.Select(d => d.MealId).ToList(),
                number_of_days = numberOfDays // Tell AI to generate for N days, not always 7
            };

            // 5. Gọi AI Service (Python)
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2); // AI service may take time
            
            var response = await httpClient.PostAsync(_aiServiceUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI Service failed: {error}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var menuPlan = JsonSerializer.Deserialize<List<AiMenuPlanItem>>(resultJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (menuPlan == null || !menuPlan.Any())
            {
                throw new Exception("AI Service returned empty or invalid menu plan");
            }

            return menuPlan;
        }

        /// <summary>
        /// Generate and save menu directly to database
        /// Note: This method is deprecated. Use GenerateMenuAsync with remainingDates instead.
        /// </summary>
        public async Task<int> GenerateAndSaveMenuAsync(Guid userId, DateOnly weekStart)
        {
            // Build a list of 7 days from weekStart for backward compatibility
            var dates = new List<DateOnly>();
            for (int i = 0; i < 7; i++)
            {
                dates.Add(weekStart.AddDays(i));
            }
            
            var menuPlan = await GenerateMenuAsync(userId, dates);
            return await SaveMenuToDb(userId, weekStart, menuPlan);
        }

        private async Task<int> SaveMenuToDb(Guid userId, DateOnly weekStart, List<AiMenuPlanItem> plan)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var menu = new WeeklyMenu
                {
                    CreatedByUserId = userId,
                    WeekStart = weekStart,
                    WeekEnd = weekStart.AddDays(6)
                };
                _context.WeeklyMenus.Add(menu);
                await _context.SaveChangesAsync();

                foreach (var day in plan)
                {
                    foreach (var mealId in day.meal_ids)
                    {
                        _context.WeeklyMenuItems.Add(new WeeklyMenuItem
                        {
                            WeeklyMenuId = menu.Id,
                            MealId = mealId,
                            DayOfWeek = day.day
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return menu.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Tính TDEE (Total Daily Energy Expenditure) dựa trên BMR và Activity Level
        /// Sử dụng Mifflin-St Jeor Equation
        /// </summary>
        private int CalculateTDEE(UserNutritionProfile profile, AppUser user)
        {
            // BMR (Basal Metabolic Rate) - Mifflin-St Jeor Equation
            // BMR = 10 * weight(kg) + 6.25 * height(cm) - 5 * age + s
            // s = +5 (male) or -161 (female)
            var age = user.Age > 0 ? user.Age : 30; // Default age nếu chưa set
            var genderFactor = user.Gender == DAL.Enums.Gender.Female ? -161 : 5;
            var bmr = 10 * (double)profile.WeightKg + 6.25 * profile.HeightCm - 5 * age + genderFactor;

            // TDEE = BMR * Activity Multiplier
            var activityMultipliers = new Dictionary<int, double>
            {
                { 1, 1.2 },  // Sedentary
                { 2, 1.375 }, // Lightly Active
                { 3, 1.55 },  // Moderately Active
                { 4, 1.725 }, // Very Active
                { 5, 1.9 }    // Extra Active
            };

            var multiplier = activityMultipliers.GetValueOrDefault((int)profile.ActivityLevel, 1.55);
            var tdee = bmr * multiplier;

            // Adjust based on goal
            switch (profile.Goal)
            {
                case DAL.Enums.FitnessGoal.FatLoss:
                    tdee *= 0.85; // Deficit 15%
                    break;
                case DAL.Enums.FitnessGoal.MuscleGain:
                    tdee *= 1.15; // Surplus 15%
                    break;
                // Maintain: no adjustment
            }

            return (int)Math.Round(tdee);
        }
    }

    // DTO Helper
    public class AiMenuPlanItem
    {
        public int day { get; set; }
        public List<int> meal_ids { get; set; } = new();
        public string reason { get; set; } = string.Empty;
    }
}
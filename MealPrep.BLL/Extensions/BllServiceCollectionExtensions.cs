using MealPrep.BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Extensions
{
    public static class BllServiceCollectionExtensions
    {
        public static IServiceCollection AddBllServices(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMealService, MealService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<INutritionLogService, NutritionLogService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAiMenuService, AiMenuService>();
            
            // Admin Services
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminPlanService, AdminPlanService>();
            services.AddScoped<IAdminSubscriptionService, AdminSubscriptionService>();
            services.AddScoped<IAdminDeliveryOrderService, AdminDeliveryOrderService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            
            // User Services
            services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
            
            // Register Meal Embedding Service (Phase 1: Optional - requires AWS SDK)
            // TODO: Uncomment when AWS Bedrock SDK is installed:
            // services.AddScoped<IMealEmbeddingService, MealEmbeddingService>();
            
            // Register HttpClient for MoMo service and AI service
            services.AddHttpClient();
            services.AddScoped<IMomoService>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<MomoPaymentService>>();
                return new MomoPaymentService(configuration, httpClient, logger);
            });

            return services;
        }
    }
}

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
            
            // Register HttpClient for MoMo service
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

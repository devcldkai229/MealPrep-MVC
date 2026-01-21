using MealPrep.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}

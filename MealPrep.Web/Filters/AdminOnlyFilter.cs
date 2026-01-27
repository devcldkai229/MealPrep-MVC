using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MealPrep.Web.Filters
{
    public class AdminOnlyFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // If user is admin and trying to access non-admin pages, redirect to Admin Dashboard
            if (context.HttpContext.User.Identity?.IsAuthenticated == true &&
                context.HttpContext.User.IsInRole("Admin"))
            {
                var controller = context.RouteData.Values["controller"]?.ToString();
                var action = context.RouteData.Values["action"]?.ToString();
                
                // List of admin controllers
                var adminControllers = new[] { "Admin", "AdminUsers", "AdminMeals", "AdminSubscriptions", "AdminPlans", "AdminDeliveryOrders", "AdminInventory" };
                
                // If accessing a non-admin controller, redirect to Admin Dashboard
                if (!adminControllers.Contains(controller) && 
                    controller != "Auth" && // Allow auth pages
                    controller != "Home" && // Allow home page
                    action != "Logout") // Allow logout
                {
                    context.Result = new RedirectToActionResult("Index", "Admin", null);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}

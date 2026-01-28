using MealPrep.BLL.Services;
using BusinessObjects.Entities;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class NutritionLogController : Controller
    {
        private readonly INutritionLogService _svc;
        private readonly IUserService _userService;

        public NutritionLogController(INutritionLogService svc, IUserService userService)
        {
            _svc = svc;
            _userService = userService;
        }

        public async Task<IActionResult> Index(DateOnly? date)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var logs = await _svc.ListAsync(userId, date);
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Meals = await _svc.GetActiveMealsAsync();
            return View(new NutritionLog { Date = DateOnly.FromDateTime(DateTime.Today), Quantity = 1 });
        }

        [HttpPost]
        public async Task<IActionResult> Create(NutritionLogVm model)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewBag.Meals = await _svc.GetActiveMealsAsync();
                return View(model);
            }
            NutritionLog nutritionLog = new NutritionLog
            {
                AppUserId = userId,
                CustomerEmail = user.Email,
                Date = model.Date,
                MealId = model.MealId,
                Quantity = model.Quantity
            };
            await _svc.CreateAsync(userId, user.Email, nutritionLog);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary(DateOnly from, DateOnly to)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sum = await _svc.SummaryAsync(userId, from, to);
            ViewBag.From = from; ViewBag.To = to;
            return View(sum);
        }
    }
}

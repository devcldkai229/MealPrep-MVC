using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class MealController : Controller
    {
        private readonly IMealService _svc;

        public MealController(IMealService svc) => _svc = svc;

        public async Task<IActionResult> Index(string? q, string? sort)
        {
            ViewBag.Query = q;
            ViewBag.Sort = sort;
            var meals = await _svc.SearchAsync(q, sort);
            return View(meals);
        }

        public async Task<IActionResult> Details(int id)
        {
            var meal = await _svc.GetAsync(id);
            if (meal == null) return NotFound();
            
            // Check if user has disliked this meal
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                var userService = HttpContext.RequestServices.GetRequiredService<IUserService>();
                ViewBag.IsDisliked = await userService.IsMealDislikedAsync(userId, id);
            }
            else
            {
                ViewBag.IsDisliked = false;
            }
            
            return View(meal);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new Meal());

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Meal meal)
        {
            if (!ModelState.IsValid) return View(meal);
            await _svc.CreateAsync(meal);
            return RedirectToAction(nameof(Index));
        }
    }
}

using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _svc;
        private readonly IRepository<DeliverySlot> _slotRepo;

        public SubscriptionController(ISubscriptionService svc, IRepository<DeliverySlot> slotRepo)
        {
            _svc = svc;
            _slotRepo = slotRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Slots = await _slotRepo.Query().Where(s => s.IsActive).ToListAsync();
            return View(new Subscription
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                MealsPerDay = 2
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subscription model, int deliverySlotId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Slots = await _slotRepo.Query().Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var id = await _svc.CreateAsync(userId, model, deliverySlotId);
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sub = await _svc.GetDetailsAsync(id, userId);
            if (sub == null) return NotFound();
            return View(sub);
        }
    }
}

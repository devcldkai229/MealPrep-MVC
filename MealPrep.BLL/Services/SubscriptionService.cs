using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<Subscription> _subRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Meal> _mealRepo;
    private readonly IRepository<DeliverySlot> _slotRepo;

    public SubscriptionService(
        IRepository<Subscription> subRepo,
        IRepository<Order> orderRepo,
        IRepository<Meal> mealRepo,
        IRepository<DeliverySlot> slotRepo)
    {
        _subRepo = subRepo;
        _orderRepo = orderRepo;
        _mealRepo = mealRepo;
        _slotRepo = slotRepo;
    }

    public async Task<int> CreateAsync(Guid userId, Subscription sub, int deliverySlotId)
    {
        var slot = await _slotRepo.Query()
            .Where(s => s.IsActive && s.Id == deliverySlotId)
            .FirstOrDefaultAsync();

        if (slot == null) throw new InvalidOperationException("Delivery slot is invalid.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (sub.StartDate < today)
            throw new InvalidOperationException("Start date must be today or later.");

        sub.AppUserId = userId;
        sub.Status = SubscriptionStatus.PendingPayment;
        // EndDate will be calculated based on Plan.DurationDays
        // For now, we'll handle this in the payment flow

        await _subRepo.AddAsync(sub);
        await _subRepo.SaveChangesAsync();

        // TODO: Generate DeliveryOrders after payment is confirmed
        // For now, we skip auto-generation
        
        /*
        var selectedMeals = await _mealRepo.Query()
            .Where(m => m.IsActive)
            .OrderBy(m => m.Calories)
            .Take(sub.MealsPerDay)
            .ToListAsync();

        if (selectedMeals.Count == 0)
            throw new InvalidOperationException("No active meals available. Please add meals first.");

        for (int i = 0; i < days; i++)
        {
            var date = sub.StartDate.AddDays(i);

            var order = new Order
            {
                AppUserId = userId,
                SubscriptionId = sub.Id,
                DeliveryDate = date,
                DeliverySlotId = deliverySlotId,
                Status = OrderStatus.Planned,
                Items = new List<OrderItem>()
            };

            for (int j = 0; j < sub.MealsPerDay; j++)
            {
                var meal = selectedMeals[j % selectedMeals.Count];
                order.Items.Add(new OrderItem
                {
                    MealId = meal.Id,
                    Quantity = 1
                });
            }

            await _orderRepo.AddAsync(order);
        }

        await _orderRepo.SaveChangesAsync();
        */
        
        return sub.Id;
    }

    public Task<Subscription?> GetDetailsAsync(int id, Guid userId)
    {
        return _subRepo.Query()
            .Include(s => s.Orders)
                .ThenInclude(o => o.DeliverySlot)
            .Include(s => s.Orders)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Meal)
            .FirstOrDefaultAsync(s => s.Id == id && s.AppUserId == userId);
    }
}
}

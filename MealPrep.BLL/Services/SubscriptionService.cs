using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly IRepository<Plan> _planRepo;
    private readonly IRepository<PlanMealTier> _tierRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<PaymentTransaction> _transactionRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IRepository<Subscription> subRepo,
        IRepository<Order> orderRepo,
        IRepository<Meal> mealRepo,
        IRepository<DeliverySlot> slotRepo,
        IRepository<Plan> planRepo,
        IRepository<PlanMealTier> tierRepo,
        IRepository<Payment> paymentRepo,
        IRepository<PaymentTransaction> transactionRepo,
        IRepository<AppUser> userRepo,
        AppDbContext dbContext,
        ILogger<SubscriptionService> logger)
    {
        _subRepo = subRepo;
        _orderRepo = orderRepo;
        _mealRepo = mealRepo;
        _slotRepo = slotRepo;
        _planRepo = planRepo;
        _tierRepo = tierRepo;
        _paymentRepo = paymentRepo;
        _transactionRepo = transactionRepo;
        _userRepo = userRepo;
        _dbContext = dbContext;
        _logger = logger;
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
            .Include(s => s.DeliveryOrders)
                .ThenInclude(o => o.DeliverySlot)
            .Include(s => s.DeliveryOrders)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Meal)
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id && s.AppUserId == userId);
    }

    public async Task<List<Plan>> GetAllPlansWithTiersAsync()
    {
        return await _planRepo.Query()
            .Include(p => p.MealTiers.Where(t => t.IsActive))
            .Where(p => p.IsActive)
            .OrderBy(p => p.DurationDays)
            .ToListAsync();
    }

    public Task<Plan?> GetPlanByIdAsync(int planId)
    {
        return _planRepo.Query()
            .Include(p => p.MealTiers.Where(t => t.IsActive))
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);
    }

    public async Task<PlanMealTier?> GetTierByIdAsync(int tierId)
    {
        return await _tierRepo.Query()
            .FirstOrDefaultAsync(t => t.Id == tierId && t.IsActive);
    }

    public async Task<decimal> CalculateTotalPriceAsync(int planId, int tierId)
    {
        var plan = await GetPlanByIdAsync(planId);
        if (plan == null) throw new InvalidOperationException("Plan not found");

        var tier = await GetTierByIdAsync(tierId);
        if (tier == null || tier.PlanId != planId) 
            throw new InvalidOperationException("Tier not found or does not belong to plan");

        return plan.BasePrice + tier.ExtraPrice;
    }

    public async Task<(Subscription subscription, Payment payment)> CreateSubscriptionWithPaymentAsync(
        Guid userId, int planId, int tierId, DateOnly startDate)
    {
        var user = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new InvalidOperationException("User not found");

        var plan = await GetPlanByIdAsync(planId);
        var tier = await GetTierByIdAsync(tierId);

        if (plan == null || tier == null || tier.PlanId != planId)
        {
            throw new InvalidOperationException("Invalid plan or tier selected.");
        }

        var totalAmount = await CalculateTotalPriceAsync(planId, tierId);

        // Create Subscription
        var subscription = new Subscription
        {
            AppUserId = userId,
            PlanId = planId,
            CustomerName = user.FullName,
            CustomerEmail = user.Email,
            MealsPerDay = tier.MealsPerDay,
            StartDate = startDate,
            Status = SubscriptionStatus.PendingPayment,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };

        await _subRepo.AddAsync(subscription);
        await _subRepo.SaveChangesAsync();

        // Create Payment
        var payment = new Payment
        {
            AppUserId = userId,
            SubscriptionId = subscription.Id,
            Amount = totalAmount,
            Currency = "VND",
            Method = "MoMo",
            Status = "Pending",
            PaymentCode = Guid.NewGuid().ToString(),
            Description = $"Thanh toan subscription {plan.Name} - {tier.MealsPerDay} meals/day",
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddHours(24)
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return (subscription, payment);
    }

    public async Task<Subscription?> ConfirmPaymentAsync(string paymentCode, string? momoOrderId = null, string? rawResponse = null)
    {
        if (string.IsNullOrWhiteSpace(paymentCode))
        {
            _logger.LogError("ConfirmPaymentAsync called with empty paymentCode");
            throw new InvalidOperationException("Payment code cannot be empty");
        }

        _logger.LogInformation("Confirming payment with code: {PaymentCode}, MoMoOrderId: {MoMoOrderId}", paymentCode, momoOrderId);

        var payment = await _paymentRepo.Query()
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.PaymentCode == paymentCode);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found with code: {PaymentCode}. Searching all payments...", paymentCode);
            
            // Debug: Log all payment codes to help troubleshoot
            var allPaymentCodes = await _paymentRepo.Query()
                .Select(p => p.PaymentCode)
                .Take(10)
                .ToListAsync();
            _logger.LogWarning("Sample payment codes in database: {Codes}", string.Join(", ", allPaymentCodes));
            
            throw new InvalidOperationException($"Payment not found with code: {paymentCode}");
        }

        _logger.LogInformation("Payment found: Id={PaymentId}, Status={Status}, Amount={Amount}, SubscriptionId={SubscriptionId}", 
            payment.Id, payment.Status, payment.Amount, payment.SubscriptionId);

        // Prevent duplicate processing
        if (payment.Status == "Paid")
        {
            _logger.LogWarning("Payment {PaymentCode} already processed at {PaidAt}", paymentCode, payment.PaidAt);
            throw new InvalidOperationException("Payment already processed");
        }

        // Only process if status is Pending
        if (payment.Status != "Pending")
        {
            _logger.LogWarning("Payment {PaymentCode} has invalid status: {Status}", paymentCode, payment.Status);
            throw new InvalidOperationException($"Payment has invalid status: {payment.Status}");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Log transaction for tracking
            if (!string.IsNullOrEmpty(momoOrderId) || !string.IsNullOrEmpty(rawResponse))
            {
                var paymentTransaction = new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    Gateway = "MoMo",
                    OrderId = momoOrderId ?? paymentCode,
                    ResponseCode = "0",
                    ResponseMessage = "Payment confirmed",
                    RawResponseJson = rawResponse
                };
                await _transactionRepo.AddAsync(paymentTransaction);
            }

            // Update Payment
            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;

            // Update Subscription
            var subscription = payment.Subscription;
            if (subscription != null)
            {
                subscription.Status = SubscriptionStatus.Active;
                subscription.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                subscription.UpdatedAt = DateTime.UtcNow;

                var plan = await _planRepo.GetByIdAsync(subscription.PlanId);
                if (plan != null)
                {
                    subscription.EndDate = subscription.StartDate.AddDays(plan.DurationDays);
                }
                else
                {
                    _logger.LogWarning("Plan {PlanId} not found for subscription {SubscriptionId}", 
                        subscription.PlanId, subscription.Id);
                }
            }
            else
            {
                _logger.LogWarning("Subscription not found for payment {PaymentCode}", paymentCode);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Payment {PaymentCode} confirmed successfully. Subscription {SubscriptionId} activated.", 
                paymentCode, subscription?.Id);
            
            return subscription;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to update payment status for {PaymentCode}", paymentCode);
            throw;
        }
    }
}
}

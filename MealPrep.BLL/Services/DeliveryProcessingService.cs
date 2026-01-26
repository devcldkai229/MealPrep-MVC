using MealPrep.BLL.DTOs;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Services
{
    public class DeliveryProcessingService : IDeliveryProcessingService
    {
        private readonly IRepository<Subscription> _subscriptionRepo;
        private readonly IRepository<DeliveryOrder> _deliveryOrderRepo;
        private readonly IRepository<DeliveryOrderItem> _deliveryOrderItemRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<Meal> _mealRepo;
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<UserDislikedMeal> _dislikedMealRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<DeliveryProcessingService> _logger;

        public DeliveryProcessingService(
            IRepository<Subscription> subscriptionRepo,
            IRepository<DeliveryOrder> deliveryOrderRepo,
            IRepository<DeliveryOrderItem> deliveryOrderItemRepo,
            IRepository<Order> orderRepo,
            IRepository<OrderItem> orderItemRepo,
            IRepository<Meal> mealRepo,
            IRepository<AppUser> userRepo,
            IRepository<UserDislikedMeal> dislikedMealRepo,
            AppDbContext context,
            ILogger<DeliveryProcessingService> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _deliveryOrderRepo = deliveryOrderRepo;
            _deliveryOrderItemRepo = deliveryOrderItemRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _mealRepo = mealRepo;
            _userRepo = userRepo;
            _dislikedMealRepo = dislikedMealRepo;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 🔄 LUỒNG CHÍNH: Generate DeliveryOrders cho ngày mai
        /// 
        /// === ĐƯỜNG ĐI DỮ LIỆU ===
        /// 1. Input: targetDate (mặc định = ngày mai)
        /// 2. Query: Lấy tất cả Subscriptions Active có StartDate <= targetDate <= EndDate
        /// 3. Loop qua từng Subscription:
        ///    a. Kiểm tra đã có DeliveryOrder cho ngày này chưa? → Skip nếu có
        ///    b. Tìm Order tương ứng (Order.DeliveryDate == targetDate)
        ///       - Nếu có Order → User đã chọn món → Copy từ OrderItems
        ///       - Nếu KHÔNG có Order → User quên chọn → Auto-assign meals
        ///    c. Tạo DeliveryOrder mới với status = Planned
        ///    d. Tạo DeliveryOrderItems tương ứng
        /// 4. SaveChanges & Return kết quả
        /// 
        /// === WORKFLOW STATUS ===
        /// Planned → Delivering → Delivered (hoặc Cancelled)
        /// </summary>
        public async Task<GenerateDeliveryOrdersResult> GenerateDeliveryOrdersForDateAsync(DateOnly? targetDate = null)
        {
            var result = new GenerateDeliveryOrdersResult();
            var deliveryDate = targetDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            _logger.LogInformation("🚀 Starting delivery order generation for date: {Date}", deliveryDate);

            try
            {
                // === BƯỚC 1: Lấy Active Subscriptions ===
                var activeSubscriptions = await _subscriptionRepo.Query()
                    .Include(s => s.AppUser)
                    .Include(s => s.Plan)
                    .Where(s =>
                        s.Status == SubscriptionStatus.Active &&
                        s.StartDate <= deliveryDate &&
                        (s.EndDate == null || s.EndDate >= deliveryDate))
                    .ToListAsync();

                _logger.LogInformation("📊 Found {Count} active subscriptions", activeSubscriptions.Count);
                result.TotalSubscriptionsProcessed = activeSubscriptions.Count;

                foreach (var subscription in activeSubscriptions)
                {
                    try
                    {
                        // === BƯỚC 2: Kiểm tra DeliveryOrder đã tồn tại chưa ===
                        var existingDeliveryOrder = await _deliveryOrderRepo.Query()
                            .AnyAsync(d => d.SubscriptionId == subscription.Id && d.DeliveryDate == deliveryDate);

                        if (existingDeliveryOrder)
                        {
                            _logger.LogDebug("⏭️ DeliveryOrder already exists for Subscription {SubId}", subscription.Id);
                            continue; // Skip nếu đã có
                        }

                        // === BƯỚC 3: Tìm Order tương ứng (User đã chọn món chưa?) ===
                        var userOrder = await _orderRepo.Query()
                            .Include(o => o.Items)
                                .ThenInclude(i => i.Meal)
                            .FirstOrDefaultAsync(o =>
                                o.AppUserId == subscription.AppUserId &&
                                o.SubscriptionId == subscription.Id &&
                                o.DeliveryDate == deliveryDate);

                        // === BƯỚC 4: Tạo DeliveryOrder ===
                        var deliveryOrder = new DeliveryOrder
                        {
                            SubscriptionId = subscription.Id,
                            DeliveryDate = deliveryDate,
                            Status = OrderStatus.Planned,
                            TotalAmount = 0, // Sẽ tính sau
                            CreatedAt = DateTime.UtcNow
                        };

                        await _deliveryOrderRepo.AddAsync(deliveryOrder);
                        await _deliveryOrderRepo.SaveChangesAsync(); // Save để có Id

                        // === BƯỚC 5: Tạo DeliveryOrderItems ===
                        if (userOrder != null && userOrder.Items.Any())
                        {
                            // ✅ User đã chọn món → Copy từ Order
                            _logger.LogInformation("✅ User {UserId} has selected meals for {Date}", 
                                subscription.AppUserId, deliveryDate);

                            foreach (var orderItem in userOrder.Items)
                            {
                                var deliveryItem = new DeliveryOrderItem
                                {
                                    DeliveryOrderId = deliveryOrder.Id,
                                    MealId = orderItem.MealId,
                                    MealNameSnapshot = orderItem.Meal?.Name ?? "Unknown",
                                    Quantity = orderItem.Quantity,
                                    UnitPrice = orderItem.Meal?.BasePrice ?? 0,
                                    CreatedAt = DateTime.UtcNow
                                };

                                await _deliveryOrderItemRepo.AddAsync(deliveryItem);
                                deliveryOrder.TotalAmount += deliveryItem.UnitPrice * deliveryItem.Quantity;
                            }
                        }
                        else
                        {
                            // ❌ User QUÊN chọn món → Auto-assign
                            _logger.LogWarning("⚠️ User {UserId} forgot to select meals. Auto-assigning...", 
                                subscription.AppUserId);

                            var autoAssignSuccess = await AutoAssignMealsForDeliveryOrderInternalAsync(
                                deliveryOrder, 
                                subscription.MealsPerDay,
                                subscription.AppUserId);

                            if (autoAssignSuccess)
                            {
                                result.TotalAutoAssignedMeals += subscription.MealsPerDay;
                            }
                        }

                        _deliveryOrderRepo.Update(deliveryOrder);
                        await _deliveryOrderRepo.SaveChangesAsync();

                        result.TotalOrdersCreated++;
                        _logger.LogInformation("✅ Created DeliveryOrder #{Id} for Subscription #{SubId}", 
                            deliveryOrder.Id, subscription.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing Subscription {SubId}", subscription.Id);
                        result.Errors.Add($"Subscription {subscription.Id}: {ex.Message}");
                    }
                }

                _logger.LogInformation("🎉 Completed! Created {Count} delivery orders", result.TotalOrdersCreated);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Fatal error in GenerateDeliveryOrdersForDateAsync");
                throw;
            }
        }

        /// <summary>
        /// 🤖 Tự động chọn món cho User quên chọn
        /// 
        /// === LOGIC AUTO-ASSIGN ===
        /// 1. Lấy danh sách món Active, sắp xếp theo:
        ///    - Calories thấp nhất (healthy meals)
        ///    - Món bán chạy (popular meals) - TODO: Add tracking
        /// 2. Random hoặc Round-robin để đa dạng
        /// 3. Tạo DeliveryOrderItems tương ứng
        /// </summary>
        private async Task<bool> AutoAssignMealsForDeliveryOrderInternalAsync(
            DeliveryOrder deliveryOrder, 
            int mealsPerDay,
            Guid? userId = null)
        {
            try
            {
                // ✅ STEP 1: Lấy danh sách món User đã chặn
                var dislikedMealIds = new List<int>();
                if (userId.HasValue)
                {
                    dislikedMealIds = await _dislikedMealRepo.Query()
                        .Where(d => d.AppUserId == userId.Value)
                        .Select(d => d.MealId)
                        .ToListAsync();

                    if (dislikedMealIds.Any())
                    {
                        _logger.LogInformation("🚫 User {UserId} has {Count} disliked meals to filter out", 
                            userId.Value, dislikedMealIds.Count);
                    }
                }

                // ✅ STEP 2: Lấy danh sách món healthy (calories thấp) VÀ loại bỏ món bị chặn
                var availableMeals = await _mealRepo.Query()
                    .Where(m => m.IsActive && !dislikedMealIds.Contains(m.Id))
                    .OrderBy(m => m.Calories)
                    .Take(mealsPerDay * 3) // Lấy nhiều hơn để random
                    .ToListAsync();

                if (availableMeals.Count == 0)
                {
                    _logger.LogError("❌ No available meals to auto-assign (after filtering disliked meals)");
                    return false;
                }

                // Random để tạo đa dạng
                var random = new Random();
                var selectedMeals = availableMeals
                    .OrderBy(x => random.Next())
                    .Take(mealsPerDay)
                    .ToList();

                foreach (var meal in selectedMeals)
                {
                    var item = new DeliveryOrderItem
                    {
                        DeliveryOrderId = deliveryOrder.Id,
                        MealId = meal.Id,
                        MealNameSnapshot = meal.Name,
                        Quantity = 1,
                        UnitPrice = meal.BasePrice,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _deliveryOrderItemRepo.AddAsync(item);
                    deliveryOrder.TotalAmount += item.UnitPrice;
                }

                await _deliveryOrderItemRepo.SaveChangesAsync();
                _logger.LogInformation("✅ Auto-assigned {Count} meals for DeliveryOrder #{Id}", 
                    mealsPerDay, deliveryOrder.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-assigning meals");
                return false;
            }
        }

        /// <summary>
        /// 🍳 Export Kitchen List - Danh sách tổng hợp cần nấu
        /// 
        /// === ĐƯỜNG ĐI DỮ LIỆU ===
        /// 1. Input: date (ngày cần export)
        /// 2. Query: Lấy tất cả DeliveryOrders có DeliveryDate = date và Status != Cancelled
        /// 3. Flatten tất cả DeliveryOrderItems
        /// 4. GroupBy MealId → Sum Quantity
        /// 5. Return: KitchenExportDto với tổng hợp
        /// 
        /// VD Output:
        /// - Gà Nướng Mật Ong: 50 suất
        /// - Cá Hồi Áp Chảo: 30 suất
        /// - Salad Gà: 25 suất
        /// </summary>
        public async Task<KitchenExportDto> GetKitchenListAsync(DateOnly date)
        {
            _logger.LogInformation("🍳 Generating kitchen list for {Date}", date);

            var deliveryOrders = await _deliveryOrderRepo.Query()
                .Include(d => d.Items)
                    .ThenInclude(i => i.Meal)
                .Where(d => d.DeliveryDate == date && d.Status != OrderStatus.Cancelled)
                .ToListAsync();

            // Flatten và GroupBy MealId
            var kitchenItems = deliveryOrders
                .SelectMany(d => d.Items)
                .Where(i => i.MealId != null) // Chỉ lấy items đã có MealId
                .GroupBy(i => new
                {
                    i.MealId,
                    i.Meal!.Name,
                    i.UnitPrice,
                    i.Meal.Calories,
                    i.Meal.Ingredients
                })
                .Select(g => new KitchenListItemDto(
                    g.Key.MealId!.Value,
                    g.Key.Name,
                    g.Sum(x => x.Quantity), // Tổng số suất cần nấu
                    g.Key.UnitPrice,
                    g.Key.Calories,
                    g.Key.Ingredients
                ))
                .OrderByDescending(x => x.TotalQuantity) // Món nhiều nhất ở đầu
                .ToList();

            var exportDto = new KitchenExportDto
            {
                DeliveryDate = date,
                TotalDeliveryOrders = deliveryOrders.Count,
                TotalMealPortions = kitchenItems.Sum(x => x.TotalQuantity),
                TotalRevenue = deliveryOrders.Sum(d => d.TotalAmount),
                Items = kitchenItems,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("✅ Kitchen list generated: {TotalOrders} orders, {TotalPortions} portions", 
                exportDto.TotalDeliveryOrders, exportDto.TotalMealPortions);

            return exportDto;
        }

        public async Task<List<DeliveryOrderDetailDto>> GetDeliveryOrdersByDateAsync(
            DateOnly date, 
            OrderStatus? status = null)
        {
            var query = _deliveryOrderRepo.Query()
                .Include(d => d.Subscription)
                    .ThenInclude(s => s!.AppUser)
                .Include(d => d.DeliverySlot)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Meal)
                .Where(d => d.DeliveryDate == date);

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            var deliveryOrders = await query
                .OrderBy(d => d.Status)
                .ThenBy(d => d.Subscription!.CustomerName)
                .ToListAsync();

            return deliveryOrders.Select(d => new DeliveryOrderDetailDto(
                d.Id,
                d.SubscriptionId,
                d.Subscription!.AppUserId,
                d.Subscription.CustomerName,
                d.Subscription.CustomerEmail,
                d.Subscription.AppUser?.PhoneNumber,
                d.DeliveryDate,
                d.DeliverySlot?.Name,
                d.Status,
                d.TotalAmount,
                d.Items.Select(i => new DeliveryOrderItemDto(
                    i.Id,
                    i.MealId,
                    i.MealNameSnapshot,
                    i.MealType,
                    i.Quantity,
                    i.UnitPrice
                )).ToList()
            )).ToList();
        }

        public async Task<bool> UpdateDeliveryOrderStatusAsync(int deliveryOrderId, OrderStatus newStatus)
        {
            var deliveryOrder = await _deliveryOrderRepo.GetByIdAsync(deliveryOrderId);
            if (deliveryOrder == null)
            {
                _logger.LogWarning("⚠️ DeliveryOrder #{Id} not found", deliveryOrderId);
                return false;
            }

            deliveryOrder.Status = newStatus;
            deliveryOrder.UpdatedAt = DateTime.UtcNow;

            _deliveryOrderRepo.Update(deliveryOrder);
            await _deliveryOrderRepo.SaveChangesAsync();

            _logger.LogInformation("✅ Updated DeliveryOrder #{Id} status to {Status}", 
                deliveryOrderId, newStatus);

            return true;
        }

        public async Task<int> BulkUpdateDeliveryOrderStatusAsync(List<int> deliveryOrderIds, OrderStatus newStatus)
        {
            var deliveryOrders = await _deliveryOrderRepo.Query()
                .Where(d => deliveryOrderIds.Contains(d.Id))
                .ToListAsync();

            foreach (var order in deliveryOrders)
            {
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                _deliveryOrderRepo.Update(order);
            }

            await _deliveryOrderRepo.SaveChangesAsync();

            _logger.LogInformation("✅ Bulk updated {Count} delivery orders to {Status}", 
                deliveryOrders.Count, newStatus);

            return deliveryOrders.Count;
        }

        public async Task<bool> AutoAssignMealsForDeliveryOrderAsync(int deliveryOrderId)
        {
            var deliveryOrder = await _deliveryOrderRepo.Query()
                .Include(d => d.Subscription)
                .FirstOrDefaultAsync(d => d.Id == deliveryOrderId);

            if (deliveryOrder == null)
            {
                return false;
            }

            return await AutoAssignMealsForDeliveryOrderInternalAsync(
                deliveryOrder, 
                deliveryOrder.Subscription!.MealsPerDay,
                deliveryOrder.Subscription.AppUserId);
        }
    }
}

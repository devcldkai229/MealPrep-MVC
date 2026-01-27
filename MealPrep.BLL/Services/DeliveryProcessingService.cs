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
        ///       - Nếu KHÔNG có Order → Skip (User cần tự chọn món)
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

                        // Chỉ xử lý nếu User đã chọn món
                        if (userOrder == null || !userOrder.Items.Any())
                        {
                            _logger.LogWarning("⚠️ User {UserId} has not selected meals for {Date}. Skipping.", 
                                subscription.AppUserId, deliveryDate);
                            continue; // Skip nếu chưa chọn món
                        }

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

            // Validation: Cannot mark as "Delivered" if delivery date is in the future
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (newStatus == OrderStatus.Delivered && deliveryOrder.DeliveryDate > today)
            {
                _logger.LogWarning("⚠️ Cannot mark DeliveryOrder #{Id} as Delivered: DeliveryDate {DeliveryDate} is in the future (Today: {Today})", 
                    deliveryOrderId, deliveryOrder.DeliveryDate, today);
                throw new InvalidOperationException(
                    $"Không thể đánh dấu đơn hàng là 'Đã giao' vì ngày giao hàng ({deliveryOrder.DeliveryDate:dd/MM/yyyy}) chưa đến. " +
                    $"Chỉ có thể đánh dấu 'Đã giao' cho các đơn hàng có ngày giao hàng <= {today:dd/MM/yyyy}.");
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

            // Validation: Cannot mark as "Delivered" if delivery date is in the future
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (newStatus == OrderStatus.Delivered)
            {
                var futureOrders = deliveryOrders.Where(d => d.DeliveryDate > today).ToList();
                if (futureOrders.Any())
                {
                    var futureDates = string.Join(", ", futureOrders.Select(d => d.DeliveryDate.ToString("dd/MM/yyyy")));
                    _logger.LogWarning("⚠️ Cannot bulk mark {Count} delivery orders as Delivered: DeliveryDates {Dates} are in the future (Today: {Today})", 
                        futureOrders.Count, futureDates, today);
                    throw new InvalidOperationException(
                        $"Không thể đánh dấu {futureOrders.Count} đơn hàng là 'Đã giao' vì ngày giao hàng chưa đến: {futureDates}. " +
                        $"Chỉ có thể đánh dấu 'Đã giao' cho các đơn hàng có ngày giao hàng <= {today:dd/MM/yyyy}.");
                }
            }

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
    }
}

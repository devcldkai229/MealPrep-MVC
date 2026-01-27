using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly AppDbContext _context;

        public UserSubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userId)
        {
            return await _context.Set<Subscription>()
                .Include(s => s.Plan)
                .Include(s => s.DeliveryOrders.OrderBy(d => d.DeliveryDate))
                .Where(s => s.AppUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetUserSubscriptionDetailsAsync(int id, Guid userId)
        {
            return await _context.Set<Subscription>()
                .Include(s => s.Plan)
                .Include(s => s.DeliveryOrders.OrderBy(d => d.DeliveryDate))
                    .ThenInclude(d => d.DeliverySlot)
                .Include(s => s.DeliveryOrders)
                    .ThenInclude(d => d.Items)
                        .ThenInclude(i => i.Meal)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id && s.AppUserId == userId);
        }

        public async Task CancelPendingSubscriptionAsync(int id, Guid userId)
        {
            var subscription = await _context.Set<Subscription>()
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id && s.AppUserId == userId);

            if (subscription == null)
            {
                throw new InvalidOperationException("Không tìm thấy gói đăng ký.");
            }

            if (subscription.Status != DAL.Enums.SubscriptionStatus.PendingPayment)
            {
                throw new InvalidOperationException("Chỉ có thể hủy gói đang ở trạng thái Chờ thanh toán.");
            }

            // Nếu đã có payment Paid thì không cho hủy (tránh case thanh toán đã thành công)
            if (subscription.Payments != null && subscription.Payments.Any(p => p.Status == "Paid"))
            {
                throw new InvalidOperationException("Gói này đã được thanh toán, không thể hủy.");
            }

            // Cập nhật trạng thái subscription
            subscription.Status = DAL.Enums.SubscriptionStatus.Cancelled;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Hủy các payment Pending (nếu có)
            if (subscription.Payments != null)
            {
                foreach (var payment in subscription.Payments.Where(p => p.Status == "Pending"))
                {
                    payment.Status = "Cancelled";
                    payment.ExpiredAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

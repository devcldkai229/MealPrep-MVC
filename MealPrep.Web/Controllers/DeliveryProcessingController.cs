using MealPrep.BLL.Services;
using MealPrep.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    /// <summary>
    /// Controller xử lý Daily Delivery Processing (Flow 5)
    /// Chỉ Admin mới access được
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DeliveryProcessingController : Controller
    {
        private readonly IDeliveryProcessingService _deliveryProcessingService;
        private readonly ILogger<DeliveryProcessingController> _logger;

        public DeliveryProcessingController(
            IDeliveryProcessingService deliveryProcessingService,
            ILogger<DeliveryProcessingController> logger)
        {
            _deliveryProcessingService = deliveryProcessingService;
            _logger = logger;
        }

        /// <summary>
        /// 🏠 Dashboard - Trang chủ Delivery Processing
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 🚀 Manual Trigger: Generate DeliveryOrders cho ngày chỉ định
        /// Thường dùng để test hoặc chạy lại khi có lỗi
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDeliveryOrders(DateOnly? targetDate)
        {
            try
            {
                _logger.LogInformation("🚀 Admin triggered delivery order generation for {Date}", 
                    targetDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

                var result = await _deliveryProcessingService.GenerateDeliveryOrdersForDateAsync(targetDate);

                if (result.Errors.Any())
                {
                    TempData["WarningMessage"] = $"Processed {result.TotalOrdersCreated} orders with {result.Errors.Count} errors.";
                    TempData["Errors"] = string.Join("; ", result.Errors);
                }
                else
                {
                    TempData["SuccessMessage"] = $"✅ Successfully generated {result.TotalOrdersCreated} delivery orders! " +
                                                 $"Auto-assigned {result.TotalAutoAssignedMeals} meals.";
                }

                return RedirectToAction(nameof(DailyOrders), new { date = targetDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating delivery orders");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 📋 Daily Orders - Xem danh sách DeliveryOrders theo ngày
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DailyOrders(DateOnly? date, OrderStatus? status)
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var deliveryOrders = await _deliveryProcessingService.GetDeliveryOrdersByDateAsync(
                selectedDate, 
                status);

            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedStatus = status;

            return View(deliveryOrders);
        }

        /// <summary>
        /// 📊 Kitchen Export - Xuất danh sách món cần nấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> KitchenExport(DateOnly? date)
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var kitchenList = await _deliveryProcessingService.GetKitchenListAsync(selectedDate);

                return View(kitchenList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating kitchen export");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 📥 Download Kitchen List as CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadKitchenList(DateOnly date)
        {
            var kitchenList = await _deliveryProcessingService.GetKitchenListAsync(date);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Meal ID,Meal Name,Total Quantity,Unit Price,Calories,Ingredients");

            foreach (var item in kitchenList.Items)
            {
                csv.AppendLine($"{item.MealId},{item.MealName},{item.TotalQuantity}," +
                              $"{item.UnitPrice},{item.Calories},\"{item.Ingredients}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"KitchenList_{date:yyyyMMdd}.csv");
        }

        /// <summary>
        /// 🔄 Update trạng thái DeliveryOrder
        /// Planned → Preparing → Delivering → Delivered
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int deliveryOrderId, 
            OrderStatus newStatus, 
            DateOnly returnDate)
        {
            try
            {
                var success = await _deliveryProcessingService.UpdateDeliveryOrderStatusAsync(
                    deliveryOrderId, 
                    newStatus);

                if (success)
                {
                    TempData["SuccessMessage"] = $"✅ Updated order status to {newStatus}";
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ Order not found";
                }

                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating status");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
        }

        /// <summary>
        /// 🔄 Bulk update trạng thái nhiều orders
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus(
            List<int> orderIds, 
            OrderStatus newStatus, 
            DateOnly returnDate)
        {
            try
            {
                var count = await _deliveryProcessingService.BulkUpdateDeliveryOrderStatusAsync(
                    orderIds, 
                    newStatus);

                TempData["SuccessMessage"] = $"✅ Updated {count} orders to {newStatus}";

                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error bulk updating");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
        }

        /// <summary>
        /// 🤖 Auto-assign meals cho DeliveryOrder
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoAssignMeals(int deliveryOrderId, DateOnly returnDate)
        {
            try
            {
                var success = await _deliveryProcessingService.AutoAssignMealsForDeliveryOrderAsync(
                    deliveryOrderId);

                if (success)
                {
                    TempData["SuccessMessage"] = "✅ Auto-assigned meals successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ Failed to auto-assign meals";
                }

                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-assigning meals");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(DailyOrders), new { date = returnDate });
            }
        }
    }
}

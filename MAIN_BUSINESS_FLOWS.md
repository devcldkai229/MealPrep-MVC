# 📋 CÁC LUỒNG NGHIỆP VỤ CHÍNH (MAIN BUSINESS FLOWS)

Tài liệu này mô tả các luồng nghiệp vụ chính của hệ thống MealPrep, không phải các thao tác CRUD cơ bản mà là các quy trình nghiệp vụ hoàn chỉnh từ đầu đến cuối.

---

## 🔐 FLOW 1: ĐĂNG KÝ & THIẾT LẬP HỒ SƠ NGƯỜI DÙNG

### Mục đích
Người dùng mới đăng ký tài khoản và thiết lập đầy đủ thông tin cá nhân, hồ sơ dinh dưỡng để sử dụng hệ thống.

### Luồng xử lý

#### 1.1. Đăng ký tài khoản
- **Controller**: `AuthController.Register()`, `SendOtp()`
- **Service**: `AuthService.SendOtpAsync()`, `RegisterAsync()`
- **Quy trình**:
  1. User nhập email → Gửi OTP qua email
  2. User nhập OTP + thông tin cơ bản (FullName, Password)
  3. Hệ thống tạo tài khoản với role "User"
  4. Tự động đăng nhập và redirect đến trang hoàn tất thông tin

#### 1.2. Hoàn tất thông tin cá nhân
- **Controller**: `AuthController.CompleteProfile()`
- **Service**: `UserService.UpdateProfileAsync()`
- **Quy trình**:
  1. User nhập: PhoneNumber, Gender, Age
  2. Lưu vào `AppUser`
  3. Redirect đến trang thiết lập hồ sơ dinh dưỡng

#### 1.3. Thiết lập hồ sơ dinh dưỡng
- **Controller**: `AuthController.SetupNutritionProfile()`
- **Service**: `UserService.UpsertNutritionProfileAsync()`
- **Quy trình**:
  1. User nhập:
     - HeightCm, WeightKg
     - Goal (FitnessGoal: LoseWeight, MaintainWeight, GainWeight)
     - ActivityLevel (Sedentary, Light, Moderate, Active, VeryActive)
     - DietPreference (Omnivore, Vegetarian, Vegan, etc.)
     - MealsPerDay (1-3)
     - CaloriesInDay (tự động tính TDEE nếu không có)
     - Notes (ghi chú đặc biệt)
     - Allergies (danh sách dị ứng)
  2. Lưu vào `UserNutritionProfile`
  3. Redirect đến Dashboard

### Entities liên quan
- `AppUser`: Thông tin cơ bản người dùng
- `UserNutritionProfile`: Hồ sơ dinh dưỡng
- `UserAllergy`: Danh sách dị ứng
- `OtpCode`: Mã OTP xác thực

---

## 💳 FLOW 2: ĐĂNG KÝ GÓI SUBSCRIPTION & THANH TOÁN

### Mục đích
Người dùng đăng ký gói meal prep (7 ngày hoặc 30 ngày) và thanh toán qua MoMo.

### Luồng xử lý

#### 2.1. Xem và chọn gói
- **Controller**: `SubscriptionController.Index()`
- **Service**: `SubscriptionService.GetAllPlansWithTiersAsync()`
- **Quy trình**:
  1. Hiển thị danh sách Plans (7-day, 30-day)
  2. Mỗi Plan có các Tiers (MealsPerDay: 1, 2, 3)
  3. **Hiện tại chỉ hỗ trợ 2 bữa/ngày** (MealsPerDay = 2)
  4. User chọn Plan + Tier + StartDate

#### 2.2. Checkout và tạo Payment
- **Controller**: `SubscriptionController.Checkout()`
- **Service**: `SubscriptionService.CreateSubscriptionWithPaymentAsync()`
- **Quy trình**:
  1. Validate: Chỉ cho phép MealsPerDay = 2
  2. Tạo `Subscription` với status = `PendingPayment`
  3. Tính EndDate = StartDate + Plan.DurationDays - 1
  4. Tạo `Payment` với status = `Pending`
  5. Tạo MoMo payment request
  6. Redirect đến MoMo payment gateway

#### 2.3. Xác nhận thanh toán (Callback)
- **Controller**: `SubscriptionController.Callback()`, `IpnCallback()`
- **Service**: `SubscriptionService.ConfirmPaymentAsync()`
- **Quy trình**:
  1. MoMo gọi callback sau khi thanh toán
  2. Verify payment signature (nếu có)
  3. Update `Payment` status = `Paid`
  4. Update `Subscription` status = `Active`
  5. Ghi `PaymentTransaction` log
  6. Redirect user đến trang chi tiết subscription

### Entities liên quan
- `Plan`: Gói đăng ký (7-day, 30-day)
- `PlanMealTier`: Tier của gói (1, 2, 3 bữa/ngày)
- `Subscription`: Đăng ký của user
- `Payment`: Thanh toán
- `PaymentTransaction`: Log giao dịch

---

## 🍽️ FLOW 3: CHỌN MÓN ĂN HÀNG TUẦN

### Mục đích
User chọn món ăn cho từng ngày trong tuần, với 2 bữa/ngày (Morning và Evening).

### Luồng xử lý

#### 3.1. Xem menu hàng tuần
- **Controller**: `MenuController.SelectMeals()`
- **Service**: `MenuService.GetWeeklySelectionAsync()`
- **Quy trình**:
  1. Kiểm tra user có subscription Active không
  2. Tính weekStart:
     - Nếu StartDate <= today → weekStart = tomorrow
     - Nếu StartDate > today → weekStart = StartDate
  3. Hiển thị 7 ngày (hoặc đến EndDate nếu subscription ngắn hơn)
  4. Mỗi ngày có 2 slots: Morning (slot index 0) và Evening (slot index 1)
  5. Load meals từ `WeeklyMenu` (nếu có) hoặc tất cả active meals
  6. **Filter món có allergen** (Hard Constraint: Zero Tolerance)
  7. Hiển thị locked meals nếu đã có order

#### 3.2. Chọn món thủ công
- **Controller**: `MenuController.SelectMealsPost()`
- **Service**: `MenuService.SaveMealSelectionsAsync()`
- **Quy trình**:
  1. User chọn món cho từng slot (Morning/Evening) của từng ngày
  2. Validate:
     - Số lượng món = MealsPerDay (2 món/ngày)
     - Ngày không bị lock (đã qua 00:00 hoặc đã có order)
  3. Tạo/Update `DeliveryOrder` cho mỗi ngày
  4. Tạo `DeliveryOrderItem` với:
     - `DeliverySlotId`: Morning (Id=1) hoặc Evening (Id=3)
     - `MealType`: "Breakfast" hoặc "Dinner"
  5. Lưu delivery address (từ form hoặc user profile)

#### 3.3. AI Menu Generation
- **Controller**: `MenuController.GenerateAiMenu()`, `AcceptAiMenu()`
- **Service**: `AiMenuService.GenerateMenuAsync()`, `MenuService.SaveMealSelectionsAsync()`
- **Quy trình**:
  1. User click "Tạo menu AI"
  2. Tìm các ngày chưa có order (remaining dates)
  3. Gọi AI Service (Python) với:
     - User profile (height, weight, goal, activity, allergies, etc.)
     - Disliked meals
     - Weekly notes (nếu có)
     - Number of days cần generate
  4. AI trả về recommendations (meal IDs cho từng ngày)
  5. Hiển thị recommendations cho user review
  6. User có thể chỉnh sửa hoặc accept
  7. Lưu vào `DeliveryOrder` và `DeliveryOrderItem`

### Entities liên quan
- `Subscription`: Gói đăng ký
- `DeliveryOrder`: Đơn giao hàng cho một ngày
- `DeliveryOrderItem`: Món ăn trong đơn (có DeliverySlotId)
- `DeliverySlot`: Khung giờ giao hàng (Morning, Evening)
- `Meal`: Món ăn
- `WeeklyMenu`: Menu hàng tuần (nếu có)
- `UserAllergy`: Dị ứng của user (để filter)

---

## 🚚 FLOW 4: TẠO ĐƠN GIAO HÀNG (DELIVERY ORDER GENERATION)

### Mục đích
Admin hoặc hệ thống tự động tạo `DeliveryOrder` từ các `Order` mà user đã chọn món, chuẩn bị cho việc giao hàng.

### Luồng xử lý

#### 4.1. Generate Delivery Orders
- **Controller**: `DeliveryProcessingController.GenerateDeliveryOrders()`
- **Service**: `DeliveryProcessingService.GenerateDeliveryOrdersForDateAsync()`
- **Quy trình**:
  1. Lấy tất cả Active Subscriptions có:
     - StartDate <= targetDate <= EndDate
  2. Với mỗi subscription:
     - Kiểm tra đã có DeliveryOrder cho ngày này chưa → Skip nếu có
     - Tìm `Order` tương ứng (Order.DeliveryDate == targetDate)
     - **Nếu có Order với Items** → Copy sang DeliveryOrder
     - **Nếu không có Order** → Skip (user chưa chọn món)
  3. Tạo `DeliveryOrder` với status = `Planned`
  4. Tạo `DeliveryOrderItem` từ `OrderItem`:
     - Copy MealId, Quantity, UnitPrice
     - Copy DeliverySlotId từ OrderItem (nếu có)
  5. Tính TotalAmount

#### 4.2. Kitchen Export
- **Controller**: `DeliveryProcessingController.KitchenExport()`
- **Service**: `DeliveryProcessingService.GetKitchenListAsync()`
- **Quy trình**:
  1. Lấy tất cả DeliveryOrders cho một ngày
  2. Group by MealId
  3. Tính tổng quantity cần nấu cho mỗi món
  4. Export CSV cho bếp

#### 4.3. Update Status
- **Controller**: `DeliveryProcessingController.UpdateStatus()`, `BulkUpdateStatus()`
- **Service**: `DeliveryProcessingService.UpdateDeliveryOrderStatusAsync()`
- **Quy trình**:
  1. Update status: `Planned` → `Preparing` → `Delivering` → `Delivered`
  2. Hỗ trợ bulk update nhiều orders cùng lúc

### Entities liên quan
- `Subscription`: Gói đăng ký
- `Order`: Order user đã chọn món
- `OrderItem`: Món trong Order
- `DeliveryOrder`: Đơn giao hàng
- `DeliveryOrderItem`: Món trong DeliveryOrder

---

## 📦 FLOW 5: GIAO HÀNG (SHIPPER DELIVERY)

### Mục đích
Shipper nhận đơn hàng, giao hàng, và upload ảnh bằng chứng.

### Luồng xử lý

#### 5.1. Xem danh sách đơn cần giao
- **Controller**: `ShipperDeliveryController.Index()`
- **Service**: `ShipperService.GetOrdersForDateAsync()`
- **Quy trình**:
  1. Shipper xem tất cả đơn đã được assign (ShipperId = userId)
  2. Filter theo ngày (nếu có)
  3. Hiển thị: DeliveryDate, Customer, Address, Status, Items

#### 5.2. Chi tiết đơn hàng
- **Controller**: `ShipperDeliveryController.Details()`
- **Service**: `ShipperService.GetOrderDetailsAsync()`
- **Quy trình**:
  1. Hiển thị chi tiết đơn: customer info, address, items
  2. Mỗi item hiển thị DeliverySlot (Morning/Evening)
  3. Form upload ảnh bằng chứng cho từng item

#### 5.3. Upload ảnh bằng chứng
- **Controller**: `ShipperDeliveryController.UploadProof()`
- **Service**: `ShipperService.UploadDeliveryProofAsync()`, `S3Service.UploadFileAsync()`
- **Quy trình**:
  1. Shipper chọn ảnh và upload
  2. Upload lên AWS S3
  3. Lưu S3 Key vào `DeliveryOrderItem.ProofImageKey`
  4. Generate presigned URL để hiển thị ngay
  5. Return JSON với presigned URL (AJAX) hoặc redirect
  6. UI tự động hiển thị ảnh và cập nhật status

#### 5.4. Hoàn thành đơn hàng
- **Controller**: `ShipperDeliveryController.CompleteOrder()`
- **Service**: `ShipperService.CompleteOrderAsync()`
- **Quy trình**:
  1. Kiểm tra tất cả items đã có proof image
  2. Update DeliveryOrder status = `Delivered`
  3. Update tất cả items status = `Delivered`

### Entities liên quan
- `DeliveryOrder`: Đơn giao hàng
- `DeliveryOrderItem`: Món trong đơn (có ProofImageKey)
- `AppUser`: Shipper user
- `S3Service`: Upload file lên AWS S3

---

## ⭐ FLOW 6: ĐÁNH GIÁ MÓN ĂN (MEAL FEEDBACK)

### Mục đích
User đánh giá món ăn đã nhận, hệ thống học hỏi và tự động chặn món rating thấp.

### Luồng xử lý

#### 6.1. Xem danh sách món cần đánh giá
- **Controller**: `MealFeedbackController.Index()`
- **Service**: `MealFeedbackService.GetPendingFeedbacksAsync()`
- **Quy trình**:
  1. Lấy tất cả DeliveryOrderItems đã delivered nhưng chưa có rating
  2. Filter trong 7 ngày gần đây (nếu không có date filter)
  3. Hiển thị: Meal name, Delivery date, Slot (Morning/Evening)

#### 6.2. Submit rating
- **Controller**: `MealFeedbackController.SubmitRating()`
- **Service**: `MealFeedbackService.SubmitMealRatingAsync()`
- **Quy trình**:
  1. User đánh giá:
     - Stars (1-5)
     - Tags (comma-separated: "spicy", "too-sweet", etc.)
     - Comments (optional)
  2. Lưu vào `MealRating`
  3. **Tự động chặn món nếu rating <= 2 sao**:
     - Thêm vào `UserDislikedMeal`
     - Món sẽ không xuất hiện trong menu selection nữa
  4. Ghi vào `NutritionLog`:
     - Calories, Protein, Carbs, Fat
     - DeliveryDate
  5. Redirect về danh sách pending feedbacks

#### 6.3. Thống kê feedback
- **Controller**: `MealFeedbackController.MySummary()`
- **Service**: `MealFeedbackService.GetUserFeedbackSummaryAsync()`
- **Quy trình**:
  1. Hiển thị:
     - Tổng số món đã đánh giá
     - Average rating
     - Top món yêu thích
     - Top món không thích

#### 6.4. Admin Report
- **Controller**: `MealFeedbackController.AdminReport()`
- **Service**: `MealFeedbackService.GetLowRatedMealsReportAsync()`
- **Quy trình**:
  1. Hiển thị món có rating thấp (average < 3 sao)
  2. Filter theo số sao
  3. Phân trang
  4. Admin có thể xem và quyết định cải thiện món

### Entities liên quan
- `DeliveryOrderItem`: Item đã delivered
- `MealRating`: Đánh giá món ăn
- `UserDislikedMeal`: Món bị chặn (tự động hoặc manual)
- `NutritionLog`: Nhật ký dinh dưỡng

---

## 📊 FLOW 7: DASHBOARD & THỐNG KÊ

### Mục đích
User và Admin xem tổng quan về subscription, đơn hàng, dinh dưỡng.

### Luồng xử lý

#### 7.1. User Dashboard
- **Controller**: `DashboardController.Index()`
- **Service**: `DashboardService.GetDashboardDataAsync()`
- **Quy trình**:
  1. Hiển thị:
     - Subscription status (Active/Pending/Cancelled)
     - Next delivery date
     - Today calories (từ NutritionLog)
     - Week calories (7 ngày gần đây)
     - Featured meals (top rated)
     - Recent orders
  2. Notification: Pending feedbacks count

#### 7.2. Admin Dashboard
- **Controller**: `AdminController.Index()`
- **Service**: `AdminDashboardService.GetDashboardStatsAsync()`, `GetRevenueByMonthAsync()`
- **Quy trình**:
  1. Hiển thị KPIs:
     - Total Users
     - Active Subscriptions
     - Today Orders
     - Today Revenue
  2. Monthly Revenue Chart:
     - Filter theo năm/tháng
     - Hiển thị daily revenue trong tháng
     - Chart.js visualization

### Entities liên quan
- `Subscription`: Đăng ký
- `DeliveryOrder`: Đơn giao hàng
- `Payment`: Thanh toán
- `NutritionLog`: Nhật ký dinh dưỡng
- `MealRating`: Đánh giá món

---

## 🔄 FLOW 8: QUẢN LÝ SUBSCRIPTION (USER)

### Mục đích
User xem và quản lý các subscription của mình.

### Luồng xử lý

#### 8.1. Xem danh sách subscriptions
- **Controller**: `UserSubscriptionsController.Index()`
- **Service**: `UserSubscriptionService.GetUserSubscriptionsAsync()`
- **Quy trình**:
  1. Hiển thị tất cả subscriptions của user
  2. Status: PendingPayment, Active, Cancelled, Expired
  3. Thông tin: Plan name, StartDate, EndDate, MealsPerDay

#### 8.2. Chi tiết subscription
- **Controller**: `UserSubscriptionsController.Details()`
- **Service**: `UserSubscriptionService.GetUserSubscriptionDetailsAsync()`
- **Quy trình**:
  1. Hiển thị:
     - Plan info
     - Payment status
     - Delivery orders (theo ngày)
     - Mỗi order hiển thị items với DeliverySlot (Morning/Evening)
  2. Actions:
     - Retry payment (nếu PendingPayment)
     - Cancel pending subscription

#### 8.3. Hủy subscription đang chờ thanh toán
- **Controller**: `UserSubscriptionsController.CancelPending()`
- **Service**: `UserSubscriptionService.CancelPendingSubscriptionAsync()`
- **Quy trình**:
  1. Chỉ cho phép hủy nếu status = `PendingPayment`
  2. Update status = `Cancelled`
  3. User có thể đăng ký gói mới

### Entities liên quan
- `Subscription`: Đăng ký
- `Payment`: Thanh toán
- `DeliveryOrder`: Đơn giao hàng
- `DeliveryOrderItem`: Món trong đơn

---

## 🔧 FLOW 9: QUẢN LÝ ADMIN

### Mục đích
Admin quản lý toàn bộ hệ thống: users, meals, plans, subscriptions, delivery orders.

### Các chức năng chính

#### 9.1. Quản lý Users
- **Controller**: `AdminUsersController`
- Xem danh sách users, chi tiết, edit, deactivate

#### 9.2. Quản lý Meals
- **Controller**: `AdminMealsController`
- CRUD meals, upload images, view ratings

#### 9.3. Quản lý Plans
- **Controller**: `AdminPlansController`
- CRUD plans và tiers

#### 9.4. Quản lý Subscriptions
- **Controller**: `AdminSubscriptionsController`
- Xem tất cả subscriptions, chi tiết, cancel

#### 9.5. Quản lý Delivery Orders
- **Controller**: `AdminDeliveryOrdersController`
- Xem danh sách orders, chi tiết, assign shipper, update status

#### 9.6. Delivery Processing
- **Controller**: `DeliveryProcessingController`
- Generate delivery orders, kitchen export, update status

#### 9.7. Dashboard
- **Controller**: `AdminController`
- KPIs, revenue charts, statistics

---

## 📝 GHI CHÚ QUAN TRỌNG

### Delivery Slot Architecture
- **DeliverySlotId** nằm ở `DeliveryOrderItem`, không phải `DeliveryOrder`
- Mỗi item có thể có slot khác nhau (Morning hoặc Evening)
- Hiện tại chỉ có 2 slots active: Morning (Id=1) và Evening (Id=3)
- Afternoon (Id=2) đã bị disable

### Meal Selection Logic
- User phải chọn đúng 2 món/ngày (MealsPerDay = 2)
- Slot index: 0 = Morning, 1 = Evening
- Món có allergen sẽ bị filter (Zero Tolerance)
- Món bị disliked (rating <= 2 sao hoặc manual) sẽ không xuất hiện

### Payment Flow
- Chỉ hỗ trợ MoMo payment gateway
- Payment status: Pending → Paid
- Subscription status: PendingPayment → Active (sau khi payment confirmed)

### Order vs DeliveryOrder
- **Order**: User đã chọn món (từ Menu Selection)
- **DeliveryOrder**: Đơn giao hàng được tạo từ Order (bởi Admin/System)
- DeliveryOrder được tạo khi user đã chọn món và admin trigger generation

---

## 🎯 TÓM TẮT CÁC FLOW CHÍNH

1. **Đăng ký & Setup Profile** → User mới thiết lập tài khoản và hồ sơ
2. **Đăng ký Subscription** → User chọn gói và thanh toán
3. **Chọn Món Ăn** → User chọn món cho từng ngày (thủ công hoặc AI)
4. **Tạo Delivery Order** → Admin/System tạo đơn giao hàng từ Order
5. **Giao Hàng** → Shipper giao hàng và upload proof
6. **Đánh Giá Món** → User đánh giá món, hệ thống học hỏi
7. **Dashboard** → User/Admin xem thống kê
8. **Quản Lý Subscription** → User quản lý gói của mình
9. **Quản Lý Admin** → Admin quản lý toàn bộ hệ thống

---

*Tài liệu này được tạo tự động dựa trên phân tích source code. Cập nhật: 2026-01-28*

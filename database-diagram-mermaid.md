```mermaid
erDiagram
    %% ============= CORE USER & AUTH =============
    AppRole ||--o{ AppUser : "has"
    AppRole {
        uuid id PK
        varchar name
    }
    
    AppUser {
        uuid id PK
        varchar email UK
        varchar full_name
        varchar password_hash
        int gender
        int age
        varchar avatar_url
        varchar phone_number
        timestamp created_at_utc
        timestamp last_login_at_utc
        boolean is_active
        uuid role_id FK
    }
    
    OtpCode {
        uuid id PK
        text email
        text code
        timestamp created_at
        timestamp expires_at
        boolean is_used
    }

    %% ============= PLAN & PRICING =============
    Plan ||--o{ PlanMealTier : "has tiers"
    Plan ||--o{ Subscription : "used in"
    
    Plan {
        int id PK
        varchar name UK
        varchar description
        int duration_days "7 or 30"
        decimal base_price
        boolean is_active
        timestamp created_at
    }
    
    PlanMealTier {
        int id PK
        int plan_id FK
        int meals_per_day "1-3"
        decimal extra_price
        boolean is_active
        timestamp created_at
    }

    %% ============= SUBSCRIPTION & PAYMENT =============
    AppUser ||--o{ Subscription : "subscribes"
    Subscription ||--o{ DeliveryOrder : "generates daily orders"
    Subscription ||--o{ Payment : "requires"
    
    Subscription {
        int id PK
        uuid app_user_id FK
        int plan_id FK
        varchar customer_name
        varchar customer_email
        int meals_per_day "1-3"
        date start_date
        date end_date
        int status "0-4"
        decimal total_amount
        boolean auto_renew
        timestamp created_at
        timestamp updated_at
    }
    
    AppUser ||--o{ Payment : "makes"
    Payment ||--o{ PaymentTransaction : "logs via"
    
    Payment {
        int id PK
        uuid app_user_id FK
        int subscription_id FK
        decimal amount
        varchar currency
        varchar method
        varchar status
        varchar payment_code UK
        varchar description
        timestamp created_at
        timestamp paid_at
        timestamp expired_at
    }
    
    PaymentTransaction {
        int id PK
        int payment_id FK
        varchar gateway
        varchar request_id
        varchar order_id
        varchar response_code
        varchar response_message
        text raw_response_json
        timestamp created_at
    }

    %% ============= DELIVERY SYSTEM (NEW) =============
    DeliverySlot ||--o{ DeliveryOrder : "scheduled in"
    DeliveryOrder ||--o{ DeliveryOrderItem : "contains"
    Meal ||--o{ DeliveryOrderItem : "delivered as"
    
    DeliverySlot {
        int id PK
        varchar name
        int capacity
        boolean is_active
    }
    
    DeliveryOrder {
        int id PK
        int subscription_id FK
        date delivery_date
        int delivery_slot_id FK
        int status "0-5"
        decimal total_amount
        varchar note
        timestamp created_at
        timestamp updated_at
    }
    
    DeliveryOrderItem {
        int id PK
        int delivery_order_id FK
        int meal_id FK
        varchar meal_name_snapshot
        varchar meal_type
        int quantity
        decimal unit_price
        timestamp created_at
    }

    %% ============= MEAL SYSTEM =============
    Meal {
        int id PK
        varchar name
        text-array ingredients
        text-array images
        varchar description
        int calories
        decimal protein
        decimal carbs
        decimal fat
        decimal base_price
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }
    
    AppUser ||--o{ WeeklyMenu : "creates"
    WeeklyMenu ||--o{ WeeklyMenuItem : "includes"
    Meal ||--o{ WeeklyMenuItem : "featured in"
    
    WeeklyMenu {
        int id PK
        uuid created_by_user_id FK
        date week_start
        date week_end
    }
    
    WeeklyMenuItem {
        int id PK
        int weekly_menu_id FK
        int meal_id FK
        int day_of_week
    }

    %% ============= NUTRITION TRACKING =============
    AppUser ||--o{ NutritionLog : "tracks"
    Meal ||--o{ NutritionLog : "logged as"
    
    NutritionLog {
        int id PK
        uuid app_user_id FK
        varchar customer_email
        date date
        int meal_id FK
        int quantity
    }
    
    AppUser ||--|| UserNutritionProfile : "has profile"
    UserNutritionProfile ||--o{ UserAllergy : "has"
    
    UserNutritionProfile {
        int id PK
        uuid app_user_id FK-UK
        int height_cm
        decimal weight_kg
        int goal
        int activity_level
        int diet_preference
        int meals_per_day
        decimal daily_budget
        varchar notes
    }
    
    UserAllergy {
        int id PK
        int user_nutrition_profile_id FK
        varchar allergy_name
    }
    
    AppUser ||--o{ UserDislikedMeal : "dislikes"
    Meal ||--o{ UserDislikedMeal : "disliked by"
    
    UserDislikedMeal {
        int id PK
        uuid app_user_id FK
        int meal_id FK
    }

    %% ============= LEGACY ORDER SYSTEM =============
    AppUser ||--o{ Order : "places legacy"
    Subscription ||--o{ Order : "generates legacy"
    DeliverySlot ||--o{ Order : "scheduled in legacy"
    Order ||--o{ OrderItem : "contains"
    Meal ||--o{ OrderItem : "ordered"
    
    Order {
        int id PK
        uuid app_user_id FK
        int subscription_id FK
        date delivery_date
        int delivery_slot_id FK
        int status
    }
    
    OrderItem {
        int id PK
        int order_id FK
        int meal_id FK
        int quantity
    }
```

## Hướng dẫn import vào Draw.io:

### Cách 1: PlantUML (Khuyến nghị)
1. Mở **draw.io** (https://app.diagrams.net/)
2. File → Import from → **Insert → Advanced → PlantUML**
3. Paste nội dung file `database-diagram.puml`
4. Click **Insert**

### Cách 2: Mermaid
1. Mở **draw.io**
2. File → Import from → **Insert → Advanced → Mermaid**
3. Paste nội dung Mermaid ở trên
4. Click **Insert**

## Giải thích màu sắc (có thể custom):

**🔵 CORE (Xanh dương)**: AppUser, AppRole, OtpCode  
**🟢 PRICING (Xanh lá)**: Plan, PlanMealTier  
**🟡 SUBSCRIPTION (Vàng)**: Subscription, Payment, PaymentTransaction  
**🟣 DELIVERY (Tím)**: DeliveryOrder, DeliveryOrderItem, DeliverySlot  
**🟠 MEAL (Cam)**: Meal, WeeklyMenu, WeeklyMenuItem  
**🔴 NUTRITION (Đỏ)**: NutritionLog, UserNutritionProfile, UserAllergy  
**⚪ LEGACY (Xám)**: Order, OrderItem

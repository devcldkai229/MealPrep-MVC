# Hướng Dẫn Chạy Migration cho KitchenInventory

## Bước 1: Dừng ứng dụng
- Dừng ứng dụng .NET đang chạy (Ctrl+C trong terminal hoặc stop trong IDE)

## Bước 2: Xóa migration file thủ công (nếu đã tạo)
- Xóa file: `MealPrep.DAL/Migrations/20260127000000_AddKitchenInventory.cs`

## Bước 3: Tạo migration tự động
```bash
dotnet ef migrations add AddKitchenInventory --project MealPrep.DAL/MealPrep.DAL.csproj --startup-project MealPrep.Web/MealPrep.Web.csproj
```

## Bước 4: Chạy migration để tạo table trong database
```bash
dotnet ef database update --project MealPrep.DAL/MealPrep.DAL.csproj --startup-project MealPrep.Web/MealPrep.Web.csproj
```

## Hoặc Cách 2: Chạy trực tiếp SQL (nếu migration file đã đúng)
Nếu migration file đã đúng, bạn có thể chạy SQL trực tiếp trong SQL Server:

```sql
CREATE TABLE [dbo].[KitchenInventories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Date] date NOT NULL,
    [MealId] int NOT NULL,
    [QuantityLimit] int NOT NULL,
    [QuantityUsed] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_KitchenInventories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KitchenInventories_Meals_MealId] FOREIGN KEY ([MealId]) REFERENCES [Meals]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_KitchenInventories_Date_MealId] ON [dbo].[KitchenInventories] ([Date], [MealId]);
CREATE INDEX [IX_KitchenInventories_MealId] ON [dbo].[KitchenInventories] ([MealId]);
```

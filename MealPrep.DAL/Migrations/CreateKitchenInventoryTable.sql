-- Migration: Create KitchenInventories Table
-- Run this SQL script directly in SQL Server Management Studio or Azure Data Studio

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KitchenInventories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[KitchenInventories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Date] date NOT NULL,
        [MealId] int NOT NULL,
        [QuantityLimit] int NOT NULL,
        [QuantityUsed] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_KitchenInventories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KitchenInventories_Meals_MealId] FOREIGN KEY ([MealId]) REFERENCES [dbo].[Meals]([Id]) ON DELETE CASCADE
    );

    -- Create unique index for Date + MealId
    CREATE UNIQUE INDEX [IX_KitchenInventories_Date_MealId] 
    ON [dbo].[KitchenInventories] ([Date], [MealId]);

    -- Create index for MealId (for faster lookups)
    CREATE INDEX [IX_KitchenInventories_MealId] 
    ON [dbo].[KitchenInventories] ([MealId]);

    PRINT 'Table KitchenInventories created successfully!';
END
ELSE
BEGIN
    PRINT 'Table KitchenInventories already exists.';
END
GO

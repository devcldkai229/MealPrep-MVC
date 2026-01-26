-- Script to add new fields to existing database
-- Run this if migrations are not in sync with database

-- Add EmbeddingJson to Meals table (if not exists)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Meals]') 
    AND name = 'EmbeddingJson'
)
BEGIN
    ALTER TABLE [Meals]
    ADD [EmbeddingJson] nvarchar(max) NULL;
    
    PRINT 'Added EmbeddingJson column to Meals table';
END
ELSE
BEGIN
    PRINT 'EmbeddingJson column already exists in Meals table';
END
GO

-- Add CaloriesInDay to UserNutritionProfiles table (if not exists)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[UserNutritionProfiles]') 
    AND name = 'CaloriesInDay'
)
BEGIN
    ALTER TABLE [UserNutritionProfiles]
    ADD [CaloriesInDay] int NULL;
    
    PRINT 'Added CaloriesInDay column to UserNutritionProfiles table';
END
ELSE
BEGIN
    PRINT 'CaloriesInDay column already exists in UserNutritionProfiles table';
END
GO

-- Mark migrations as applied (if not already)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260124070332_UpdateModel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260124070332_UpdateModel', '9.0.0');
    PRINT 'Marked migration 20260124070332_UpdateModel as applied';
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260125034727_UpdateMealAndUserNutritionProfile')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260125034727_UpdateMealAndUserNutritionProfile', '9.0.0');
    PRINT 'Marked migration 20260125034727_UpdateMealAndUserNutritionProfile as applied';
END
GO

PRINT 'Migration script completed successfully!';

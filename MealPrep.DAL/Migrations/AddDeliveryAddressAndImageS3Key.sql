-- =============================================
-- Migration: Add Delivery Address and Image S3 Key
-- Description: 
--   1. Add DeliveryAddress to Users table (for customer delivery address)
--   2. Add DeliveryAddress, ImageS3Key, and DeliveredAt to DeliveryOrderItems table
--      - DeliveryAddress: Snapshot of delivery address at order creation time
--      - ImageS3Key: S3 key for delivery confirmation image uploaded by shipper
--      - DeliveredAt: Timestamp when shipper marks delivery as completed
-- Date: 2026-01-26
-- =============================================

USE [MealPrepDB]; -- Replace with your actual database name
GO

BEGIN TRANSACTION;
GO

-- =============================================
-- 1. Add DeliveryAddress column to Users table
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') 
    AND name = 'DeliveryAddress'
)
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [DeliveryAddress] NVARCHAR(500) NULL;
    
    PRINT 'Added DeliveryAddress column to Users table';
END
ELSE
BEGIN
    PRINT 'DeliveryAddress column already exists in Users table';
END
GO

-- =============================================
-- 2. Add DeliveryAddress column to DeliveryOrderItems table
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'DeliveryAddress'
)
BEGIN
    ALTER TABLE [dbo].[DeliveryOrderItems]
    ADD [DeliveryAddress] NVARCHAR(500) NULL;
    
    PRINT 'Added DeliveryAddress column to DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT 'DeliveryAddress column already exists in DeliveryOrderItems table';
END
GO

-- =============================================
-- 3. Add ImageS3Key column to DeliveryOrderItems table
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'ImageS3Key'
)
BEGIN
    ALTER TABLE [dbo].[DeliveryOrderItems]
    ADD [ImageS3Key] NVARCHAR(500) NULL;
    
    PRINT 'Added ImageS3Key column to DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT 'ImageS3Key column already exists in DeliveryOrderItems table';
END
GO

-- =============================================
-- 4. Add DeliveredAt column to DeliveryOrderItems table
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'DeliveredAt'
)
BEGIN
    ALTER TABLE [dbo].[DeliveryOrderItems]
    ADD [DeliveredAt] DATETIME2 NULL;
    
    PRINT 'Added DeliveredAt column to DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT 'DeliveredAt column already exists in DeliveryOrderItems table';
END
GO

-- =============================================
-- 5. Update existing seed data (if any)
-- =============================================
-- Update existing users to set DeliveryAddress to NULL (if not already set)
-- This is safe as the column is nullable
UPDATE [dbo].[Users]
SET [DeliveryAddress] = NULL
WHERE [DeliveryAddress] IS NULL;
GO

-- =============================================
-- Verification: Check if all columns were added successfully
-- =============================================
PRINT '';
PRINT '=== Migration Verification ===';
PRINT '';

-- Check Users table
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') 
    AND name = 'DeliveryAddress'
)
BEGIN
    PRINT '✓ DeliveryAddress column exists in Users table';
END
ELSE
BEGIN
    PRINT '✗ DeliveryAddress column NOT found in Users table';
END

-- Check DeliveryOrderItems table
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'DeliveryAddress'
)
BEGIN
    PRINT '✓ DeliveryAddress column exists in DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT '✗ DeliveryAddress column NOT found in DeliveryOrderItems table';
END

IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'ImageS3Key'
)
BEGIN
    PRINT '✓ ImageS3Key column exists in DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT '✗ ImageS3Key column NOT found in DeliveryOrderItems table';
END

IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryOrderItems]') 
    AND name = 'DeliveredAt'
)
BEGIN
    PRINT '✓ DeliveredAt column exists in DeliveryOrderItems table';
END
ELSE
BEGIN
    PRINT '✗ DeliveredAt column NOT found in DeliveryOrderItems table';
END

PRINT '';
PRINT '=== Migration Completed ===';
PRINT '';

COMMIT TRANSACTION;
GO

-- =============================================
-- Rollback Script (if needed)
-- =============================================
/*
BEGIN TRANSACTION;
GO

-- Remove columns from DeliveryOrderItems
ALTER TABLE [dbo].[DeliveryOrderItems] DROP COLUMN [DeliveredAt];
ALTER TABLE [dbo].[DeliveryOrderItems] DROP COLUMN [ImageS3Key];
ALTER TABLE [dbo].[DeliveryOrderItems] DROP COLUMN [DeliveryAddress];

-- Remove column from Users
ALTER TABLE [dbo].[Users] DROP COLUMN [DeliveryAddress];

COMMIT TRANSACTION;
GO
*/

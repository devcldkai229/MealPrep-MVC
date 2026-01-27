-- ============================================
-- Script to create a Shipper user
-- ============================================

-- Step 1: Ensure the Shipper role exists (if not already created)
-- INSERT INTO Roles VALUES('22222222-2222-2222-2222-222222333333', 'Shipper')

-- Step 2: Create Shipper User
-- Password: shipper123
-- IMPORTANT: You MUST generate a real BCrypt hash and replace the placeholder below!
-- 
-- Quick ways to generate hash:
-- 1. Run PowerShell script: .\generate_shipper_password.ps1
-- 2. Visit: https://bcrypt-generator.com/ (password: shipper123, rounds: 11)
-- 3. Use C# in your project: BCrypt.Net.BCrypt.HashPassword("shipper123", 11)

INSERT INTO Users (
    Id,
    Email,
    FullName,
    PasswordHash,
    Gender,
    Age,
    PhoneNumber,
    RoleId,
    IsActive,
    CreatedAtUtc,
    AvatarUrl,
    DeliveryAddress
) VALUES (
    '33333333-3333-3333-3333-333333333333', -- User GUID (change to NEWID() if you want auto-generated)
    'shipper@mealprep.com', -- Email (change as needed)
    'Nguyễn Văn Shipper', -- Full Name (change as needed)
    '$2a$11$F9X9iNQBsrP84FtQoKBhEuw.AqPUd8wBvsXEUAr1XzIoYTCI8oCku', -- PasswordHash for "shipper123" (verified and ready to use)
    0, -- Gender: 0 = Unknown, 1 = Male, 2 = Female
    25, -- Age (change as needed)
    '0901234568', -- Phone Number (change as needed)
    '22222222-2222-2222-2222-222222333333', -- RoleId (Shipper role GUID - must match the role created above)
    1, -- IsActive: 1 = true, 0 = false
    GETUTCDATE(), -- CreatedAtUtc: current UTC time
    '', -- AvatarUrl: empty string
    NULL -- DeliveryAddress: NULL (optional, can be set later)
);

-- ============================================
-- How to generate BCrypt hash:
-- ============================================
-- Option 1: Online tool
--   Visit: https://bcrypt-generator.com/
--   Enter password: shipper123
--   Rounds: 11
--   Copy the generated hash and replace $2a$11$REPLACE_WITH_ACTUAL_BCRYPT_HASH above
--
-- Option 2: C# code (run in a C# console app)
--   Install: Install-Package BCrypt.Net-Next
--   Code:
--     using BCrypt.Net;
--     string hash = BCrypt.Net.BCrypt.HashPassword("shipper123", 11);
--     Console.WriteLine(hash);
--
-- Option 3: Use existing hash from seed data as reference
--   Admin password hash example: $2a$11$BgEVIOqTroolit.QXo8ZVedAuTsoAevkQVuyc/AhK02D9iDFQvimu
--   (This is for password "Admin123!" - do NOT use this for shipper)

-- ============================================
-- If user already exists, UPDATE password hash:
-- ============================================
-- UPDATE Users 
-- SET PasswordHash = '$2a$11$F9X9iNQBsrP84FtQoKBhEuw.AqPUd8wBvsXEUAr1XzIoYTCI8oCku'
-- WHERE Email = 'shipper@mealprep.com' AND RoleId = '22222222-2222-2222-2222-222222333333';
--
-- This will update the password for existing shipper user to "shipper123"

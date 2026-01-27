# PowerShell script to generate BCrypt hash for shipper password
# This script creates a temporary C# console app to generate the hash

Write-Host "Generating BCrypt hash for password 'shipper123'..." -ForegroundColor Cyan

# Create a temporary console app
$tempDir = "temp_hash_generator"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null
Set-Location $tempDir

# Create console app
dotnet new console -n HashGenerator --force | Out-Null
Set-Location HashGenerator

# Add BCrypt package
dotnet add package BCrypt.Net-Next | Out-Null

# Create Program.cs with hash generator
@"
using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "shipper123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        
        Console.WriteLine("========================================");
        Console.WriteLine("BCrypt Hash Generator");
        Console.WriteLine("========================================");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine();
        
        // Verify
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine($"Verification: {(isValid ? "SUCCESS ✓" : "FAILED ✗")}");
        Console.WriteLine();
        Console.WriteLine("Copy the hash above and update create_shipper_user.sql");
    }
}
"@ | Out-File -FilePath "Program.cs" -Encoding UTF8

# Run the generator
Write-Host "`nRunning hash generator...`n" -ForegroundColor Yellow
dotnet run

# Cleanup
Set-Location ..\..
Remove-Item -Recurse -Force $tempDir

Write-Host "`nDone! Copy the hash and update create_shipper_user.sql" -ForegroundColor Green

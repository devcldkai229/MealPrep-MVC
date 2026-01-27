# PowerShell script to generate BCrypt hash
# Requires BCrypt.Net-Next package

# Install package if needed
# dotnet add package BCrypt.Net-Next

# Create a temporary C# file to generate hash
$tempScript = @"
using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "shipper123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        Console.WriteLine("Password: " + password);
        Console.WriteLine("Hash: " + hash);
        
        // Verify
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine("Verification: " + isValid);
    }
}
"@

$tempScript | Out-File -FilePath "temp_hash_gen.cs" -Encoding UTF8
Write-Host "Created temp_hash_gen.cs - Run: dotnet run temp_hash_gen.cs"

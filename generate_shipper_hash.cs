// Quick script to generate BCrypt hash for shipper password
// Run this in a C# console app or use it as reference

using BCrypt.Net;

string password = "shipper123";
string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"BCrypt Hash: {hash}");

// Verify the hash works
bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
Console.WriteLine($"Verification: {isValid}");

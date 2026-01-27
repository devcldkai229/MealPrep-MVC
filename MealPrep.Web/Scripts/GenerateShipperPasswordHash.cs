//// Script to generate BCrypt hash for shipper password
//// Run this in Package Manager Console or create a simple console app
//// Usage: Copy this code and run in a C# console with BCrypt.Net-Next package

//using BCrypt.Net;
//using System;

//class Program
//{
//    static void Main()
//    {
//        string password = "shipper123";
//        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        
//        Console.WriteLine("========================================");
//        Console.WriteLine("Password Hash Generator for Shipper");
//        Console.WriteLine("========================================");
//        Console.WriteLine($"Password: {password}");
//        Console.WriteLine($"BCrypt Hash: {hash}");
//        Console.WriteLine();
        
//        // Verify the hash
//        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
//        Console.WriteLine($"Verification: {(isValid ? "SUCCESS" : "FAILED")}");
//        Console.WriteLine();
//        Console.WriteLine("Copy the hash above and use it in create_shipper_user.sql");
//    }
//}

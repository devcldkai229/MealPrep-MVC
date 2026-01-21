using MealPrep.BLL.Exceptions;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly Guid _userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private const int OTP_EXPIRY_MINUTES = 5;

        public AuthService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendOtpAsync(string email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new EmailAlreadyExistsException(email);
            }

            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            var oldOtps = await _context.OtpCodes
                .Where(o => o.Email == email)
                .ToListAsync();
            _context.OtpCodes.RemoveRange(oldOtps);

            var otp = new OtpCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                Code = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                IsUsed = false
            };

            _context.OtpCodes.Add(otp);
            await _context.SaveChangesAsync();

            try
            {
                Console.WriteLine($"[OTP] Sending OTP email to: {email}");
                Console.WriteLine($"[OTP] Code: {otpCode}");
                await _emailService.SendOtpEmailAsync(email, otpCode);
                Console.WriteLine($"[OTP] Email sent successfully to: {email}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[ERROR] Failed to send OTP email to {email}");
                //Console.WriteLine($"[ERROR] Exception: {ex.GetType().Name}");
                //Console.WriteLine($"[ERROR] Message: {ex.Message}");
                //Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                //Console.WriteLine($"[OTP FALLBACK] OTP for {email}: {otpCode} (Email failed, use this code for testing)");
                
                throw new Exception($"Failed to send OTP email: {ex.Message}", ex);
            }
        }

        public async Task<AuthResponse> RegisterAsync(string email, string fullName, string password, string otpCode)
        {
            var otp = await _context.OtpCodes
                .Where(o => o.Email == email && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
            {
                throw new OtpNotSentException(email);
            }

            if (otp.ExpiresAt < DateTime.UtcNow || otp.Code != otpCode)
            {
                throw new InvalidOtpException();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new EmailAlreadyExistsException(email);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                PasswordHash = passwordHash,
                RoleId = _userRoleId,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            otp.IsUsed = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FindAsync(_userRoleId);

            return new AuthResponse
            (
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                RoleId: user.RoleId,
                RoleName: role?.Name ?? "User",
                IsActive: user.IsActive,
                AvatarUrl: user.AvatarUrl
            );
        }

        public async Task<AuthResponse> LoginAsync(string Email, string Password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                throw new InvalidCredentialsException();
            }

            if (!user.IsActive)
            {
                throw new AccountDeactivatedException();
            }

            if (!BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            user.LastLoginAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponse(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                RoleId: user.RoleId,
                RoleName: user.Role.Name,
                IsActive: user.IsActive,
                AvatarUrl: user.AvatarUrl
            );
        }
    }
}

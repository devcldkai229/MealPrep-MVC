using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{

    public sealed record AuthResponse (
        Guid UserId,
        string Email,
        string FullName,
        Guid RoleId,
        string RoleName,
        bool IsActive,
        string? AvatarUrl
    );

public interface IAuthService
    {
        /// <summary>
        /// Gửi mã OTP đến email để xác thực đăng ký
        /// </summary>
        /// <exception cref="Exceptions.EmailAlreadyExistsException">Email đã tồn tại</exception>
        Task SendOtpAsync(string email);

        /// <summary>
        /// Đăng ký user mới với role User mặc định (cần xác thực OTP)
        /// </summary>
        /// <exception cref="Exceptions.InvalidOtpException">Mã OTP không đúng hoặc đã hết hạn</exception>
        /// <exception cref="Exceptions.OtpNotSentException">Chưa gửi OTP</exception>
        Task<AuthResponse> RegisterAsync(string email, string fullName, string password, string otpCode);

        /// <summary>
        /// Đăng nhập và trả về thông tin user để tạo cookie
        /// </summary>
        /// <exception cref="Exceptions.InvalidCredentialsException">Email hoặc password không đúng</exception>
        /// <exception cref="Exceptions.AccountDeactivatedException">Tài khoản đã bị vô hiệu hóa</exception>
        Task<AuthResponse> LoginAsync(string Email, string Password);
    }
}

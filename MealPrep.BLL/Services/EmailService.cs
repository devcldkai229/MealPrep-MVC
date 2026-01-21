using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUser = configuration["EmailSettings:SmtpUser"] ?? "kag40222@gmail.com";
            _smtpPassword = configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = configuration["EmailSettings:FromEmail"] ?? _smtpUser;
            _fromName = configuration["EmailSettings:FromName"] ?? "MealPrep Service";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            await SendMessageAsync(message);
        }

        public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            await SendMessageAsync(message);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "Mã xác thực đăng ký - MealPrep Service";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ background: #fff; border: 2px dashed #667eea; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 8px; margin: 20px 0; color: #667eea; border-radius: 8px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 20px 0; color: #856404; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🍽️ MealPrep Service</h1>
            <p>Xác thực tài khoản của bạn</p>
        </div>
        <div class=""content"">
            <h2>Xin chào!</h2>
            <p>Cảm ơn bạn đã đăng ký sử dụng dịch vụ MealPrep. Để hoàn tất quá trình đăng ký, vui lòng nhập mã OTP dưới đây:</p>
            
            <div class=""otp-code"">{otpCode}</div>
            
            <p>Mã OTP này có hiệu lực trong <strong>5 phút</strong>.</p>
            
            <div class=""warning"">
                ⚠️ <strong>Lưu ý:</strong> Không chia sẻ mã này với bất kỳ ai. MealPrep sẽ không bao giờ yêu cầu mã OTP qua điện thoại hoặc email.
            </div>
            
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 MealPrep Service. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

            await SendHtmlEmailAsync(toEmail, subject, htmlBody);
        }

        private async Task SendMessageAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            
            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                
                if (!string.IsNullOrEmpty(_smtpUser) && !string.IsNullOrEmpty(_smtpPassword))
                {
                    await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                }
                
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email send error: {ex.Message}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

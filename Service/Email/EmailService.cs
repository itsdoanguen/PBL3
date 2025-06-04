using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace PBL3.Service.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
        string GetForgotPasswordEmailBody(string userName, string newPassword);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var smtpServer = _configuration["SmtpServer"];
            var smtpPort = int.Parse(_configuration["SmtpPort"]);
            var smtpUser = _configuration["SmtpUser"];
            var smtpPass = _configuration["SmtpPass"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("PBL3 System", smtpUser));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public string GetForgotPasswordEmailBody(string userName, string newPassword)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; border: 1px solid #eee; border-radius: 8px; padding: 24px; background: #fafbfc;'>
                <h2 style='color: #2d8cf0;'>Xin chào {userName ?? "bạn"},</h2>
                <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản trên hệ thống <b>PBL3</b>.</p>
                <p><b>Mật khẩu mới của bạn là:</b></p>
                <div style='font-size: 1.2em; background: #f3f6fa; border-radius: 4px; padding: 12px 16px; margin: 12px 0; color: #333; letter-spacing: 2px; font-weight: bold;'>{newPassword}</div>
                <p>Vui lòng đăng nhập và đổi mật khẩu mới ngay để đảm bảo an toàn cho tài khoản.</p>
                <hr style='margin: 24px 0;'>
                <div style='color: #888; font-size: 0.95em;'>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này hoặc liên hệ quản trị viên.</div>
                <div style='margin-top: 24px; color: #2d8cf0; font-weight: bold;'>PBL3 Team</div>
            </div>";
        }
    }
}

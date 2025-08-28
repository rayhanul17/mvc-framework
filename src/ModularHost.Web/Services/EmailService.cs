using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ModularHost.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
        Task SendEmailAsync(string to, string subject, string htmlMessage, string from);
        Task SendConfirmationEmailAsync(string email, string confirmationLink);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task Send2FACodeEmailAsync(string email, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var fromEmail = _configuration["Email:FromEmail"];
            await SendEmailAsync(to, subject, htmlMessage, fromEmail);
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage, string from)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromName = _configuration["Email:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(from, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var subject = "Confirm your email";
            var htmlMessage = $@"
                <h2>Welcome to ModularHost!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}' style='background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirm Email</a></p>
                <p>If you didn't create an account, you can safely ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                <hr>
                <p style='font-size: 12px; color: gray;'>ModularHost Team</p>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Reset your password";
            var htmlMessage = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to set a new password:</p>
                <p><a href='{resetLink}' style='background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                <p>This link will expire in 1 hour.</p>
                <hr>
                <p style='font-size: 12px; color: gray;'>ModularHost Team</p>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task Send2FACodeEmailAsync(string email, string code)
        {
            var subject = "Your two-factor authentication code";
            var htmlMessage = $@"
                <h2>Two-Factor Authentication</h2>
                <p>Your verification code is:</p>
                <h1 style='font-size: 32px; color: #3b82f6; letter-spacing: 5px; text-align: center; padding: 20px; background-color: #f3f4f6; border-radius: 5px;'>{code}</h1>
                <p>Enter this code to complete your login. This code will expire in 5 minutes.</p>
                <p>If you didn't request this code, please secure your account immediately.</p>
                <hr>
                <p style='font-size: 12px; color: gray;'>ModularHost Team</p>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
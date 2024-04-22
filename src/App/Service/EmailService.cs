using MailKit.Net.Smtp;
using MimeKit;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Config;

namespace old_planner_api.src.App.Service
{
    public class EmailService : IEmailService
    {
        private readonly MailboxAddress _senderEmail;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderPassword;
        private readonly ILogger<IEmailService> _logger;

        public EmailService(
            EmailServiceSettings emailServiceSettings,
            ILogger<IEmailService> logger
            )

        {
            _senderEmail = new MailboxAddress(emailServiceSettings.SenderName, emailServiceSettings.SenderEmail);
            _senderPassword = emailServiceSettings.SenderPassword;
            _smtpPort = emailServiceSettings.SmtpPort;
            _smtpServer = emailServiceSettings.SmtpServer;
            _logger = logger;
        }

        public async Task SendMessage(string email, string subject, string message)
        {
            try
            {
                using var emailMessage = new MimeMessage();
                emailMessage.Subject = subject;
                emailMessage.From.Add(_senderEmail);
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Body = new TextPart()
                {
                    Text = message
                };

                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_senderEmail.Address, _senderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task SendMessage(string fromEmail, string toEmail, string subject, string message)
        {
            try
            {
                using var emailMessage = new MimeMessage();
                emailMessage.Subject = subject;
                emailMessage.From.Add(new MailboxAddress("", fromEmail));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Body = new TextPart()
                {
                    Text = message
                };

                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_senderEmail.Address, _senderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
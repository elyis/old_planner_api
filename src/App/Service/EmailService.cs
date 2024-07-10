using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Config;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;

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

        public async Task<List<EmailMessageInfo>> GetMessages(string email, string access_token, string refresh_token, int offset, int count, EmailProvider emailProvider)
        {
            var messages = new List<EmailMessageInfo>();
            string imapServer = "imap.gmail.com";
            int port = 993;

            if(emailProvider == EmailProvider.MailRu)
                imapServer = "imap.mail.ru";

            using var client = new ImapClient();
            try
            {
                await ConnectToServer(client, email, access_token, imapServer, port);
                await FetchMessages(client, messages, offset, count);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }

            return messages;
        }
        private async Task ConnectToServer(ImapClient client, string email, string accessToken, string imapServer, int port)
        {
            await client.ConnectAsync(imapServer, port, SecureSocketOptions.SslOnConnect);
            var oauth2 = new SaslMechanismOAuth2(email, accessToken);
            await client.AuthenticateAsync(oauth2);
            await client.Inbox.OpenAsync(FolderAccess.ReadOnly);
        }

        private async Task FetchMessages(ImapClient client, List<EmailMessageInfo> messages, int offset, int count)
        {
            int messageCount = client.Inbox.Count;
            int start = offset;
            int end = Math.Min(start + count - 1, messageCount - 1);

            var messageSummaries = await client.Inbox.FetchAsync(start, end, MessageSummaryItems.All);
            foreach (var summary in messageSummaries)
            {
                try
                {
                    var message = await client.Inbox.GetMessageAsync(summary.Index);
                    messages.Add(new EmailMessageInfo(message, summary.Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch message with UID {summary.Index}: {ex.Message}");
                }
            }
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

        public async Task DeleteMessages(string email, string accessToken, List<int> messageIndexes, EmailProvider emailProvider)
        {
            string imapServer = "imap.gmail.com";
            int port = 993;

            if(emailProvider == EmailProvider.MailRu)
                imapServer = "imap.mail.ru";

            using var client = new ImapClient();
            try
            {
                await ConnectToServer(client, email, accessToken, imapServer, port);
                await client.Inbox.OpenAsync(FolderAccess.ReadWrite);

                foreach (var messageIndex in messageIndexes)
                {
                    var message = await client.Inbox.GetMessageAsync(messageIndex);
                    await client.Inbox.AddFlagsAsync(messageIndex, MessageFlags.Deleted, true);
                }

                await client.Inbox.ExpungeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete messages: {ex.Message}");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }

        public async Task SendMessage(
            string fromEmail,
            string senderName,
            string toEmail,
            string toName,
            string subject,
            string message,
            string password,
            EmailProvider emailProvider)
        {

            string smtpServer = "smtp.gmail.com";
            int port = 587;

            if(emailProvider == EmailProvider.MailRu)
            {
                smtpServer = "smtp.mail.ru";
                port = 465;
            }

            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(senderName, fromEmail));
                emailMessage.To.Add(new MailboxAddress(toName, toEmail));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(fromEmail, password);
                await client.AuthenticateAsync(oauth2);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception e)
            {

            }
        }
    }
}
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.App.IService
{
    public interface IEmailService
    {
        Task SendMessage(string email, string subject, string message);
        Task SendMessage(string fromEmail, string toEmail, string subject, string message);
        Task SendMessage(string fromEmail, string senderName, string toEmail, string toName, string subject, string message, string password, EmailProvider emailProvider);
        Task<List<EmailMessageInfo>> GetMessages(string email, string access_token, string refresh_token, int offset, int count, EmailProvider emailProvider);
        Task DeleteMessages(string email, string accessToken, List<int> messageIndexes, EmailProvider emailProvider);
    }
}
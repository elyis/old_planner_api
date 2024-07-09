using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.App.IService
{
    public interface IEmailService
    {
        Task SendMessage(string email, string subject, string message);
        Task SendMessage(string fromEmail, string toEmail, string subject, string message);
        Task SendMessage(string fromEmail, string senderName, string toEmail, string toName, string subject, string message, string password);
        Task<List<EmailMessageInfo>> GetMessages(string email, string access_token, string refresh_token, int offset = 0, int count = 10);
        Task DeleteMessages(string email, string accessToken, List<int> messageIndexes);
    }
}
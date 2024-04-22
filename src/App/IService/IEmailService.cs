namespace old_planner_api.src.App.IService
{
    public interface IEmailService
    {
        Task SendMessage(string email, string subject, string message);
        Task SendMessage(string fromEmail, string toEmail, string subject, string message);
    }
}
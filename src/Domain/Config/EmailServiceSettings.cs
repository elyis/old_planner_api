namespace old_planner_api.src.Domain.Entities.Config
{
    public class EmailServiceSettings
    {
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderPassword { get; set; }
    }
}
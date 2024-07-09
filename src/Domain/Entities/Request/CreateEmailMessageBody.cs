namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateEmailMessageBody
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
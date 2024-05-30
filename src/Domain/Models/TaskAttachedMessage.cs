namespace old_planner_api.src.Domain.Models
{
    public class TaskAttachedMessage
    {
        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
        public ChatMessage Message { get; set; }
        public Guid MessageId { get; set; }
    }
}
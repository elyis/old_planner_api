namespace old_planner_api.src.Domain.Models
{
    public class TaskChat
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }
        public TaskModel Task { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
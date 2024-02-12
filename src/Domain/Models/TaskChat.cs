namespace old_planner_api.src.Domain.Models
{
    public class TaskChat
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }
        public TaskModel Task { get; set; }

        public List<TaskChatMessage> Messages { get; set; } = new();
        public List<TaskChatMembership> Memberships { get; set; } = new();
    }
}
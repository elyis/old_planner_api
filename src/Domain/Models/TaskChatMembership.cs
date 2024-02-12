namespace old_planner_api.src.Domain.Models
{
    public class TaskChatMembership
    {
        public Guid ChatId { get; set; }
        public TaskChat Chat { get; set; }

        public Guid ParticipantId { get; set; }
        public UserModel Participant { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}
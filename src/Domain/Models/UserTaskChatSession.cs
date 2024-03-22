namespace old_planner_api.src.Domain.Models
{
    public class UserTaskChatSession
    {
        public Guid Id { get; set; }
        public UserSession Session { get; set; }
        public Guid SessionId { get; set; }

        public TaskChatMembership ChatMembership { get; set; }
        public Guid ChatMembershipId { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}
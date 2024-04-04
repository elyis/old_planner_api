namespace old_planner_api.src.Domain.Models
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }

        public List<UserChatSession> ChatSessions { get; set; } = new();
        public List<UserTaskChatSession> TaskChatSessions { get; set; } = new();
    }
}
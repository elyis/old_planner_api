namespace old_planner_api.src.Domain.Models
{
    public class ChatMembership
    {
        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}
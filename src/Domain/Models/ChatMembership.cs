using System.ComponentModel.DataAnnotations.Schema;

namespace old_planner_api.src.Domain.Models
{
    public class ChatMembership
    {
        public Guid Id { get; set; }
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        [ForeignKey("ChatId")]
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;

        public List<UserChatSession> UserChatSessions { get; set; } = new();
    }
}
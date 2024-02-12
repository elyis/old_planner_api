using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Models
{
    public class Chat
    {
        public Guid Id { get; set; }

        [StringLength(64, MinimumLength = 1)]
        public string Name { get; set; }
        public string Type { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public List<ChatMembership> ChatMemberships { get; set; } = new();
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace old_planner_api.src.Domain.Models
{
    public class TaskChatMembership
    {
        public Guid Id { get; set; }
        [ForeignKey("ChatId")]
        public Guid ChatId { get; set; }
        public TaskChat Chat { get; set; }

        [ForeignKey("ParticipantId")]
        public Guid ParticipantId { get; set; }
        public UserModel Participant { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}
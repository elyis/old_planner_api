using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class SentMessageBody
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
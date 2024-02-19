using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateChatBody
    {
        [StringLength(64, MinimumLength = 1)]
        [Required]
        public string Name { get; set; }
    }
}
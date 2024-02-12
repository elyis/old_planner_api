using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateChatBody
    {
        [StringLength(64, MinimumLength = 1)]
        [Required]
        public string Name { get; set; }

        [EnumDataType(typeof(ChatType))]
        public ChatType Type { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
    }
}
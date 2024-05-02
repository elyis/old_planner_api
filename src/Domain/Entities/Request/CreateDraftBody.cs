using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateDraftBody
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }

        [EnumDataType(typeof(TaskType))]
        public TaskType Type { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public Guid? ModifiedTaskId { get; set; }
    }
}
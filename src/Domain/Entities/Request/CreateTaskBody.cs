using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateTaskBody
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Range(0, 10)]
        public int PriorityOrder { get; set; }

        [EnumDataType(typeof(TaskState))]
        public TaskState Status { get; set; }

        [EnumDataType(typeof(TaskType))]
        public TaskType Type { get; set; }

        [DataType(DataType.DateTime)]
        public string? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public string? EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }

        public List<Guid> MessageIds { get; set; } = new();
    }
}
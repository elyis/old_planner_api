using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateTaskBody
    {
        [MaxLength(128)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int PriorityOrder { get; set; }
        [EnumDataType(typeof(TaskState))]
        public TaskState Status { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        
        [MaxLength(7)]
        public string? HexColor { get; set; }
    }
}
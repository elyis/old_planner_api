using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Domain.Models
{
    public class TaskDraft
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAtDate { get; set; } = DateTime.UtcNow;

        public TaskModel? ModifiedTask { get; set;}
        public Guid? ModifiedTaskId { get; set; }   

        public TaskDraftBody ToTaskDraftBody()
        {
            return new TaskDraftBody
            {
                Id = Id,
                Title = Title,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate?.ToString("s"),
                EndDate = EndDate?.ToString("s"),
                TaskId = ModifiedTaskId
            };
        }
    }
}
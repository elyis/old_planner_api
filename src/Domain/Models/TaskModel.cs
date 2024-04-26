using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Models
{
    public class TaskModel
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int PriorityOrder { get; set; }
        public string Status { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAtDate { get; set; } = DateTime.UtcNow;
        public bool IsDraft { get; set; }

        public TaskModel? DraftOfTask { get; set; }
        public Guid? DraftOfTaskId { get; set; }

        public BoardColumn Column { get; set; }
        public Guid ColumnId { get; set; }

        public DeletedTask? DeletedTask { get; set; }

        public UserModel Creator { get; set; }
        public Guid CreatorId { get; set; }

        public Chat Chat { get; set; }
        public Guid ChatId { get; set; }


        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Title = Title,
                Description = Description,
                HexColor = HexColor,
                PriorityOrder = PriorityOrder,
                Status = Enum.Parse<TaskState>(Status),
                StartDate = StartDate?.ToString("s"),
                EndDate = EndDate?.ToString("s"),
                ChatId = Chat.Id
            };
        }
    }
}
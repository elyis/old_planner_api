using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Domain.Models
{
    public class DeletedTask
    {
        public Guid Id { get; set; }
        public DateTime ExistBeforeDate { get; set; }

        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }

        public DeletedTaskBody ToDeletedTaskBody()
        {
            return new DeletedTaskBody
            {
                Id = Id,
                ExistBeforeDate = ExistBeforeDate.ToString("s"),
                Task = Task.ToTaskBody()
            };
        }
    }
}
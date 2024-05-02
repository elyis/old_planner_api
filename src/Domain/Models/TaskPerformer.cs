namespace old_planner_api.src.Domain.Models
{
    public class TaskPerformer
    {
        public UserModel Performer { get; set; }
        public Guid PerformerId { get; set; }
        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
    }
}
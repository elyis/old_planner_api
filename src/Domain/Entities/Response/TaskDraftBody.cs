namespace old_planner_api.src.Domain.Entities.Response
{
    public class TaskDraftBody
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? HexColor { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public Guid? TaskId { get; set; }   
    }
}
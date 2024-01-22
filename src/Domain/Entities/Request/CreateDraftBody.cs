namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateDraftBody
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? HexColor { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public Guid? ModifiedTaskId { get; set; }   
    }
}
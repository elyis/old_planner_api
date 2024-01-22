namespace old_planner_api.src.Domain.Entities.Response
{
    public class DeletedTaskBody
    {
        public Guid Id { get; set; }
        public string ExistBeforeDate { get; set; }

        public TaskBody Task { get; set;}

    }
}
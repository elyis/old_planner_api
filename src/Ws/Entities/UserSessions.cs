namespace old_planner_api.src.Ws.Entities
{
    public class UserSessions
    {
        public Guid UserId { get; set; }
        public List<Guid> SessionIds { get; set; } = new();
    }
}
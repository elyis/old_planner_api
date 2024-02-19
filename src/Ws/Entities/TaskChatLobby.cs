namespace old_planner_api.src.Ws.Entities
{
    public class TaskChatLobby
    {
        public List<TaskChatSession> ActiveConnections { get; set; } = new();
        public List<Guid> ChatUsers { get; set; } = new();
    }
}
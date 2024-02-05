namespace old_planner_api.src.WebSockets.Entities
{
    public class TaskChatLobby
    {
        public List<UserTaskChatConnection> Connections { get; set; } = new();
    }
}
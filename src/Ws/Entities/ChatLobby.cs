namespace old_planner_api.src.Ws.Entities
{
    public class ChatLobby
    {
        public List<ChatSession> ActiveConnections { get; set; } = new();
        public List<Guid> ChatUsers { get; set; } = new();
    }
}
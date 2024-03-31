using System.Collections.Concurrent;

namespace old_planner_api.src.Ws.Entities
{
    public class TaskChatLobby
    {
        public ConcurrentDictionary<Guid, TaskChatSession> ActiveSessions { get; set; } = new();
        public List<Guid> AllChatUsers { get; set; } = new();
    }
}
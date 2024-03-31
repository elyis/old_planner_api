using System.Collections.Concurrent;

namespace old_planner_api.src.Ws.Entities
{
    public class ChatLobby
    {
        public ConcurrentDictionary<Guid, ChatSession> ActiveSessions { get; set; } = new();
        public List<Guid> AllChatUsers { get; set; } = new();
    }
}
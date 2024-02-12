using System.Net.WebSockets;
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Ws.Entities
{
    public class TaskChatSession
    {
        public ChatUserInfo User { get; set; }
        public WebSocket Ws { get; set; }
    }
}
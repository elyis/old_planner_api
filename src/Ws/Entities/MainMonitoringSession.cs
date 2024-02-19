using System.Net.WebSockets;
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Ws.Entities
{
    public class MainMonitoringSession
    {
        public WebSocket Socket { get; set; }
        public List<ChatMessageInfo> Messages { get; set; } = new();
    }
}
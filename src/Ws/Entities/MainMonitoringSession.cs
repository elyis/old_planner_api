using System.Net.WebSockets;

namespace old_planner_api.src.Ws.Entities
{
    public class MainMonitoringSession
    {
        public Guid SessionId { get; set; }
        public WebSocket Socket { get; set; }
    }
}
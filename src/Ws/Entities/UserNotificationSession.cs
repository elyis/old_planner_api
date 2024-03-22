using System.Net.WebSockets;

namespace old_planner_api.src.Ws.Entities
{
    public class UserNotificationSession
    {
        public WebSocket Socket { get; set; }
        public Guid SessionId { get; set; }
    }
}
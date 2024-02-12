using System.Net.WebSockets;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IHandler
{
    public interface IChatHandler
    {
        Task Invoke(UserModel user, ChatMembership chatMembership, Chat chat, List<ChatSession> sessions, ChatSession currentSsession);
        Task SendMessageToAll(IEnumerable<ChatSession> connections, byte[] bytes, WebSocketMessageType messageType);
    }
}
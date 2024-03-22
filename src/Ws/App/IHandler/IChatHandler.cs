using System.Net.WebSockets;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IHandler
{
    public interface IChatHandler
    {
        Task Invoke(UserModel user, ChatMembership chatMembership, Chat chat, ChatLobby lobby, ChatSession currentSession, UserChatSession userChatSession);
        Task SendMessageToAll(IEnumerable<ChatSession> connections, MessageBody message, WebSocketMessageType messageType, IEnumerable<Guid> userIds, Chat chat);
    }
}
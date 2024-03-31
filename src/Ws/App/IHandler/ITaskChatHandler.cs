using System.Net.WebSockets;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IHandler
{
    public interface ITaskChatHandler
    {
        Task Invoke(UserModel user, TaskChatMembership chatMembership, TaskChat chat, TaskChatLobby lobby, TaskChatSession currentSession, UserTaskChatSession userChatSession);
        Task SendMessage(IEnumerable<TaskChatSession> connections, MessageBody message, WebSocketMessageType messageType, IEnumerable<Guid> userIds, TaskChat chat);
    }
}
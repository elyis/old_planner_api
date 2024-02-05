using System.Net.WebSockets;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.WebSockets.Entities;

namespace old_planner_api.src.WebSockets.App.IHandler
{
    public interface ITaskChatHandler
    {
        Task Invoke(UserModel user, UserChatHistory userChatHistory, TaskChat chat, List<UserTaskChatConnection> connections, UserTaskChatConnection currentConnection);
        Task SendMessageToAll(IEnumerable<UserTaskChatConnection> connections, byte[] bytes, WebSocketMessageType messageType);
    }
}
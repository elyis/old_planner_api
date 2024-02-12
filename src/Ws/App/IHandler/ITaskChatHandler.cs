using System.Net.WebSockets;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IHandler
{
    public interface ITaskChatHandler
    {
        Task Invoke(UserModel user, TaskChatMembership userChatHistory, TaskChat chat, List<TaskChatSession> connections, TaskChatSession currentConnection);
        Task SendMessageToAll(IEnumerable<TaskChatSession> connections, byte[] bytes, WebSocketMessageType messageType);
    }
}
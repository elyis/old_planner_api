using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface ITaskChatConnectionService
    {
        TaskChatLobby AddConnection(Guid chatId, TaskChatSession userConnection, List<Guid> userIds);
        void RemoveConnection(Guid chatId, TaskChatSession userConnection);
        TaskChatLobby? GetConnections(Guid chatId);
    }
}
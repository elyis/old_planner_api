using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface ITaskChatConnectionService
    {
        TaskChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds);
        bool LobbyIsExist(Guid chatId);
        TaskChatLobby? AddSessionToLobby(Guid chatId, TaskChatSession session);
        void RemoveConnection(Guid chatId, TaskChatSession session);
        TaskChatLobby? GetConnections(Guid chatId);
    }
}
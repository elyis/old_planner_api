using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface ITaskChatConnectionService
    {
        ChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds);
        bool LobbyIsExist(Guid chatId);
        ChatLobby? AddSessionToLobby(Guid chatId, ChatSession session);
        void RemoveConnection(Guid chatId, ChatSession session);
        ChatLobby? GetConnections(Guid chatId);
    }
}
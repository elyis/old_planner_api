using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface IChatService
    {
        ChatLobby AddConnection(Guid chatId, ChatSession session, List<Guid> userIds);
        void RemoveConnection(Guid chatId, ChatSession session);
        ChatLobby? GetConnections(Guid chatId);
    }
}
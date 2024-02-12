using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface IChatService
    {
        List<ChatSession> AddConnection(Guid chatId, ChatSession userConnection);
        void RemoveConnection(Guid chatId, ChatSession session);
        List<ChatSession> GetConnections(Guid chatId);
    }
}
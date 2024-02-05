using old_planner_api.src.WebSockets.Entities;

namespace old_planner_api.src.WebSockets.App.IService
{
    public interface ITaskChatService
    {
        List<UserTaskChatConnection> AddConnection(Guid chatId, UserTaskChatConnection userConnection);
        void RemoveConnection(Guid chatId, UserTaskChatConnection userConnection);
        List<UserTaskChatConnection> GetConnections(Guid chatId);
    }
}
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface ITaskChatService
    {
        List<TaskChatSession> AddConnection(Guid chatId, TaskChatSession userConnection);
        void RemoveConnection(Guid chatId, TaskChatSession userConnection);
        List<TaskChatSession> GetConnections(Guid chatId);
    }
}
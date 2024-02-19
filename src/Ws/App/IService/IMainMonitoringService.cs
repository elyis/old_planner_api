using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface IMainMonitoringService
    {
        MainMonitoringSession AddConnection(Guid userId, MainMonitoringSession session);
        MainMonitoringSession? GetConnections(Guid userId);
        bool RemoveConnection(Guid userId);
        Task SendMessage(Guid userId, ChatMessageInfo message);
    }
}
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface INotificationService
    {
        UserNotificationSession AddConnection(Guid userId, UserNotificationSession session);
        UserNotificationSession? GetConnections(Guid userId);
        bool RemoveConnection(Guid userId);
        Task<bool> SendMessage(Guid userId, ChatMessageInfo message);
    }
}
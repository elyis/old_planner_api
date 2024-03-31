using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface INotificationService
    {
        UserNotificationSession AddUserSession(Guid userId, UserNotificationSession session);
        IEnumerable<UserNotificationSession> GetUserSessions(Guid userId);
        bool RemoveSession(Guid userId, Guid sessionId);
        Task<IEnumerable<Guid>> SendMessageToSessions(Guid userId, List<Guid> sessionIds, byte[] bytes);
        Task SendMessageToAllUserSessions(Guid userId, byte[] bytes);
    }
}
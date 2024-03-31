using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.IService
{
    public interface IMainMonitoringService
    {
        MainMonitoringSession AddUserSession(Guid userId, MainMonitoringSession session);
        IEnumerable<MainMonitoringSession> GetUserSessions(Guid userId);
        bool RemoveSession(Guid userId, Guid sessionId);
        Task<IEnumerable<Guid>> SendMessageToSessions(Guid userId, List<Guid> sessionIds, byte[] bytes);
        Task SendMessageToAllUserSessions(Guid userId, byte[] bytes);
    }
}
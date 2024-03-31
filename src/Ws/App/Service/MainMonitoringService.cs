using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class MainMonitoringService : IMainMonitoringService
    {
        private readonly ILogger<MainMonitoringService> _logger;
        private ConcurrentDictionary<Guid, List<MainMonitoringSession>> _activeUserSessions { get; set; } = new();

        public MainMonitoringService(ILogger<MainMonitoringService> logger)
        {
            _logger = logger;
        }

        public MainMonitoringSession AddUserSession(Guid userId, MainMonitoringSession session)
        {
            if (_activeUserSessions.TryGetValue(userId, out var sessions))
            {
                var existingConnection = sessions.FirstOrDefault(e => e.SessionId == session.SessionId);
                if (existingConnection == null)
                {
                    sessions.Add(session);
                    _logger.LogInformation($"Main monitoring connection is added");
                }
                return existingConnection ?? session;
            }

            _activeUserSessions.TryAdd(userId, new List<MainMonitoringSession> { session });
            _logger.LogInformation($"Main monitoring create for user {userId}");
            return session;
        }

        public IEnumerable<MainMonitoringSession> GetUserSessions(Guid userId)
        {
            return _activeUserSessions.TryGetValue(userId, out var sessions) ? sessions : new List<MainMonitoringSession>();
        }

        public bool RemoveSession(Guid userId, Guid sessionId)
        {
            if (_activeUserSessions.TryGetValue(userId, out var sessions))
            {
                var session = sessions.FirstOrDefault(e => e.SessionId == sessionId);
                if (session != null)
                {
                    sessions.Remove(session);
                    _logger.LogInformation($"Main monitoring connection is removed");
                }
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Guid>> SendMessageToSessions(Guid userId, List<Guid> ignoredSessions, byte[] bytes)
        {
            var sessionsNotReceivedMessage = new List<Guid>();

            var userSessions = GetUserSessions(userId);
            if (!userSessions.Any())
                return sessionsNotReceivedMessage;

            var sessions = userSessions.Where(e => !ignoredSessions.Contains(e.SessionId));

            foreach (var session in sessions)
            {
                var currentSession = sessions.FirstOrDefault(e => e.SessionId == session.SessionId);
                if (currentSession == null || !await SendMessage(currentSession.Socket, bytes, WebSocketMessageType.Text))
                    sessionsNotReceivedMessage.Add(session.SessionId);
            }

            return sessionsNotReceivedMessage;
        }

        public async Task SendMessageToAllUserSessions(Guid userId, byte[] bytes)
        {
            var sessions = GetUserSessions(userId);

            foreach (var session in sessions)
                await SendMessage(session.Socket, bytes, WebSocketMessageType.Text);
        }

        private async Task<bool> SendMessage(WebSocket socket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                    await socket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception e) when (e is JsonSerializationException || e is WebSocketException)
            {
                _logger.LogError($"{e.Message}");
                return false;
            }

            return true;
        }
    }
}
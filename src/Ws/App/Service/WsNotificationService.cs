using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class WsNotificationService : INotificationService
    {
        private readonly ILogger<WsNotificationService> _logger;
        private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, UserNotificationSession>> _activeUserSessions { get; set; } = new();

        public WsNotificationService(ILogger<WsNotificationService> logger)
        {
            _logger = logger;
        }

        public UserNotificationSession AddUserSession(Guid userId, UserNotificationSession session)
        {
            if (_activeUserSessions.TryGetValue(userId, out var sessions))
            {
                sessions.TryGetValue(session.SessionId, out var existingSession);
                if (existingSession == null)
                {
                    sessions.TryAdd(session.SessionId, session);
                    _logger.LogInformation($"Main monitoring connection is added");
                }

                return existingSession ?? session;
            }

            var newSessions = new ConcurrentDictionary<Guid, UserNotificationSession>();
            newSessions.TryAdd(session.SessionId, session);

            _activeUserSessions.TryAdd(userId, newSessions);
            _logger.LogInformation($"Main monitoring create for user {userId}");
            return session;
        }

        public IEnumerable<UserNotificationSession> GetUserSessions(Guid userId)
        {
            var sessions = _activeUserSessions.GetValueOrDefault(userId);
            return sessions?.Values ?? new List<UserNotificationSession>();
        }

        public bool RemoveSession(Guid userId, Guid sessionId)
        {
            if (_activeUserSessions.TryGetValue(userId, out var sessions))
            {
                sessions.TryRemove(sessionId, out var _);
                _logger.LogInformation($"Notification connection is removed from user {userId}");
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
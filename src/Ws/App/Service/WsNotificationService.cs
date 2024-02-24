using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class WsNotificationService : INotificationService
    {
        private readonly ILogger<MainMonitoringService> _logger;
        private ConcurrentDictionary<Guid, UserNotificationSession> _sessions { get; set; } = new();

        public WsNotificationService(ILogger<MainMonitoringService> logger)
        {
            _logger = logger;
        }

        public UserNotificationSession AddConnection(Guid userId, UserNotificationSession session)
        {
            var userNotificationSession = _sessions.GetOrAdd(userId, session);
            _logger.LogInformation($"user notification session created for {userId}");
            return userNotificationSession;
        }

        public UserNotificationSession? GetConnections(Guid userId)
        {
            return _sessions.TryGetValue(userId, out var session) ? session : null;
        }

        public bool RemoveConnection(Guid userId)
        {
            if (_sessions.TryRemove(userId, out var _))
            {
                _logger.LogInformation($"user notification session is removed for {userId}");
                return true;
            }

            return false;
        }

        public async Task<bool> SendMessage(Guid userId, ChatMessageInfo message)
        {
            try
            {
                var session = GetConnections(userId);
                if (session == null)
                    return await Task.FromResult(false);

                var socket = session.Socket;

                if (socket.State == WebSocketState.Open)
                {
                    var serializableString = SerializeObject(message);
                    var bytes = Encoding.UTF8.GetBytes(serializableString);

                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    return true;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError($"{e.Message}");
            }
            catch (WebSocketException e)
            {
                _logger.LogError($"{e.Message}");
            }

            return false;
        }

        private string SerializeObject<T>(T obj) => JsonConvert.SerializeObject(obj);

    }
}
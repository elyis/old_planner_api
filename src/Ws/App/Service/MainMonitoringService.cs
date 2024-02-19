using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class MainMonitoringService : IMainMonitoringService
    {
        private readonly ILogger<MainMonitoringService> _logger;
        private ConcurrentDictionary<Guid, MainMonitoringSession> _sessions { get; set; } = new();

        public MainMonitoringService(ILogger<MainMonitoringService> logger)
        {
            _logger = logger;
        }

        public MainMonitoringSession AddConnection(Guid userId, MainMonitoringSession session)
        {
            var monitoringSession = _sessions.GetOrAdd(userId, session);
            _logger.LogInformation($"Main monitoring connection is added");
            return monitoringSession;
        }

        public MainMonitoringSession? GetConnections(Guid userId)
        {
            return _sessions.TryGetValue(userId, out var session) ? session : null;
        }

        public bool RemoveConnection(Guid userId)
        {
            if (_sessions.TryRemove(userId, out var _))
            {
                _logger.LogInformation($"Main monitoring connection is removed");
                return true;
            }

            return false;
        }

        public async Task SendMessage(Guid userId, ChatMessageInfo message)
        {
            try
            {
                var session = GetConnections(userId);
                if (session == null)
                {
                    await Task.CompletedTask;
                    return;
                }

                var socket = session.Socket;

                if (socket.State == WebSocketState.Open)
                {
                    var serializableString = SerializeObject(message);
                    var bytes = Encoding.UTF8.GetBytes(serializableString);

                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
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
        }

        private string SerializeObject<T>(T obj) => JsonConvert.SerializeObject(obj);
    }
}
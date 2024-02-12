using System.Collections.Concurrent;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class TaskChatService : ITaskChatService
    {
        private readonly ILogger<TaskChatService> _logger;
        private ConcurrentDictionary<Guid, TaskChatLobby> _taskChats { get; set; } = new();

        public TaskChatService(ILogger<TaskChatService> logger)
        {
            _logger = logger;
        }

        public List<TaskChatSession> AddConnection(Guid chatId, TaskChatSession userConnection)
        {
            var chatLobby = new TaskChatLobby();
            chatLobby = _taskChats.GetOrAdd(chatId, chatLobby);
            var connections = chatLobby.Connections;

            var existingConnection = connections.FirstOrDefault(e => e.User.Id == userConnection.User.Id);
            if (existingConnection == null)
                connections.Add(userConnection);

            _logger.LogInformation($"connection is added {userConnection.User.Email}");
            return connections;
        }

        public List<TaskChatSession> GetConnections(Guid chatId)
        {
            if (_taskChats.TryGetValue(chatId, out var lobby))
                return lobby.Connections;

            return new List<TaskChatSession>();
        }

        public void RemoveConnection(Guid chatId, TaskChatSession userConnection)
        {
            if (_taskChats.TryGetValue(chatId, out var chat))
            {
                var existingConnection = chat.Connections
                    .FirstOrDefault(e => e.User.Id == userConnection.User.Id);

                if (existingConnection != null)
                {
                    chat.Connections.Remove(existingConnection);
                    if (!chat.Connections.Any())
                        _taskChats.Remove(chatId, out var _);
                }

                _logger.LogInformation($"connection is deleted {userConnection.User.Email}");
            }
        }
    }
}
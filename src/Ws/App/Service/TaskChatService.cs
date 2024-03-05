using System.Collections.Concurrent;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class TaskChatService : ITaskChatService
    {
        private readonly ILogger<TaskChatService> _logger;
        private ConcurrentDictionary<Guid, TaskChatLobby> _chats { get; set; } = new();

        public TaskChatService(ILogger<TaskChatService> logger)
        {
            _logger = logger;
        }

        public TaskChatLobby AddConnection(Guid chatId, TaskChatSession session, List<Guid> userIds)
        {
            var chatLobby = new TaskChatLobby();
            chatLobby = _chats.GetOrAdd(chatId, chatLobby);
            var connections = chatLobby.ActiveConnections;

            var existingConnection = connections.FirstOrDefault(e => e.User.Id == session.User.Id);
            if (existingConnection == null)
                connections.Add(session);

            chatLobby.ChatUsers = userIds;

            _logger.LogInformation($"connection is added {session.User.Identifier}");
            return chatLobby;
        }

        public TaskChatLobby? GetConnections(Guid chatId)
        {
            return _chats.TryGetValue(chatId, out var lobby) ? lobby : null;
        }

        public void RemoveConnection(Guid chatId, TaskChatSession userConnection)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                var existingConnection = chat.ActiveConnections
                    .FirstOrDefault(e => e.User.Id == userConnection.User.Id);

                if (existingConnection != null)
                {
                    chat.ActiveConnections.Remove(existingConnection);
                    if (!chat.ActiveConnections.Any())
                        _chats.Remove(chatId, out var _);
                }

                _logger.LogInformation($"connection is deleted {userConnection.User.Identifier}");
            }
        }
    }
}
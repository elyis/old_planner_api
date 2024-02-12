using System.Collections.Concurrent;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private ConcurrentDictionary<Guid, ChatLobby> _chats { get; set; } = new();

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public List<ChatSession> AddConnection(Guid chatId, ChatSession session)
        {
            var chatLobby = new ChatLobby();
            chatLobby = _chats.GetOrAdd(chatId, chatLobby);
            var connections = chatLobby.Connections;

            var existingConnection = connections.FirstOrDefault(e => e.User.Id == session.User.Id);
            if (existingConnection == null)
                connections.Add(session);

            _logger.LogInformation($"connection is added {session.User.Email}");
            return connections;
        }

        public List<ChatSession> GetConnections(Guid chatId)
        {
            if (_chats.TryGetValue(chatId, out var lobby))
                return lobby.Connections;

            return new List<ChatSession>();
        }

        public void RemoveConnection(Guid chatId, ChatSession session)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                var existingConnection = chat.Connections
                    .FirstOrDefault(e => e.User.Id == session.User.Id);

                if (existingConnection != null)
                {
                    chat.Connections.Remove(existingConnection);
                    if (!chat.Connections.Any())
                        _chats.Remove(chatId, out var _);
                }

                _logger.LogInformation($"connection is deleted {session.User.Email}");
            }
        }
    }
}
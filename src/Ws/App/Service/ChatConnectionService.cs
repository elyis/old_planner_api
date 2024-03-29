using System.Collections.Concurrent;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class ChatConnectionService : IChatConnectionService
    {
        private readonly ILogger<ChatConnectionService> _logger;
        private ConcurrentDictionary<Guid, ChatLobby> _chats { get; set; } = new();

        public ChatConnectionService(ILogger<ChatConnectionService> logger)
        {
            _logger = logger;
        }

        public ChatLobby AddConnection(Guid chatId, ChatSession session, List<Guid> userIds)
        {
            var chatLobby = new ChatLobby();
            chatLobby = _chats.GetOrAdd(chatId, chatLobby);
            var connections = chatLobby.ActiveConnections;

            var existingConnection = connections.FirstOrDefault(e => e.User.Id == session.User.Id);
            if (existingConnection == null)
                connections.Add(session);

            chatLobby.ChatUsers = userIds;

            _logger.LogInformation($"connection is added {session.User.Identifier}");
            return chatLobby;
        }

        public ChatLobby? GetConnections(Guid chatId)
        {
            return _chats.TryGetValue(chatId, out var lobby) ? lobby : null;
        }

        public void RemoveConnection(Guid chatId, ChatSession session)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                var existingConnection = chat.ActiveConnections
                    .FirstOrDefault(e => e.User.Id == session.User.Id);

                if (existingConnection != null)
                {
                    chat.ActiveConnections.Remove(existingConnection);
                    if (!chat.ActiveConnections.Any())
                        _chats.Remove(chatId, out var _);
                }

                _logger.LogInformation($"connection is deleted {session.User.Identifier}");
            }
        }
    }
}
using System.Collections.Concurrent;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Service
{
    public class TaskChatConnectionService : ITaskChatConnectionService
    {
        private readonly ILogger<TaskChatConnectionService> _logger;
        private ConcurrentDictionary<Guid, ChatLobby> _chatLobbies { get; set; } = new();

        public TaskChatConnectionService(ILogger<TaskChatConnectionService> logger)
        {
            _logger = logger;
        }

        public ChatLobby? AddSessionToLobby(Guid chatId, ChatSession session)
        {
            if (_chatLobbies.TryGetValue(chatId, out var lobby) && !lobby.ActiveSessions.ContainsKey(session.SessionId))
            {
                lobby.ActiveSessions.TryAdd(session.SessionId, session);
                _logger.LogInformation($"connection is added {session.User.Identifier}");
                return lobby;
            }

            return null;
        }

        public ChatLobby? GetConnections(Guid chatId)
        {
            return _chatLobbies.TryGetValue(chatId, out var lobby) ? lobby : null;
        }

        public void RemoveConnection(Guid chatId, ChatSession session)
        {
            if (_chatLobbies.TryGetValue(chatId, out var chat))
            {
                if (chat.ActiveSessions.TryRemove(session.SessionId, out var _))
                {
                    if (!chat.ActiveSessions.Any())
                        _chatLobbies.Remove(chatId, out var _);
                }
            }
            _logger.LogInformation($"connection is deleted {session.SessionId}");
        }

        public ChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds)
        {
            if (_chatLobbies.TryGetValue(chatId, out var _))
                return null;

            return _chatLobbies.GetOrAdd(chatId, new ChatLobby { AllChatUsers = allUserIds });
        }

        public bool LobbyIsExist(Guid chatId)
        {
            return _chatLobbies.ContainsKey(chatId);
        }
    }
}
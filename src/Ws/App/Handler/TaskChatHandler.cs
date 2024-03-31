using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.App.IHandler;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Handler
{
    public class TaskChatHandler : ITaskChatHandler
    {
        private readonly ITaskChatRepository _chatRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskChatHandler> _logger;

        public TaskChatHandler(
            INotificationService notificationService,
            ITaskChatRepository chatRepository,
            ILogger<TaskChatHandler> logger
        )
        {
            _notificationService = notificationService;
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task Invoke(
            UserModel user,
            TaskChatMembership chatMembership,
            TaskChat chat,
            TaskChatLobby lobby,
            TaskChatSession currentConnection,
            UserTaskChatSession userChatSession
        )
        {
            await Loop(lobby, currentConnection, user, chat, chatMembership, userChatSession);
        }

        private async Task Loop(
            TaskChatLobby lobby,
            TaskChatSession currentSession,
            UserModel user,
            TaskChat chat,
            TaskChatMembership chatMembership,
            UserTaskChatSession userChatSession
        )
        {
            var ws = currentSession.Ws;
            string? errorMessage = null;
            DateTime? dateLastViewingMessage = null;


            var sessions = lobby.ActiveSessions.Select(e => e.Value);
            var allUserIds = lobby.AllChatUsers;
            var activeSessionIds = lobby.ActiveSessions.Select(e => e.Key);

            dateLastViewingMessage = await ProcessWebSocketState(ws, sessions, allUserIds, chat, user, CancellationToken.None);

            await CloseWebSocket(ws, errorMessage);
            await UpdateLastViewingDate(chatMembership, userChatSession, dateLastViewingMessage);
        }

        public async Task SendMessage(
            IEnumerable<TaskChatSession> sessions,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> userIds,
            TaskChat chat
        )
        {
            var chatMessageBody = new ChatMessageInfo
            {
                ChatId = chat.Id,
                ChatType = ChatType.Task,
                Message = message
            };

            var connectedSessionIds = sessions.Select(e => e.SessionId);
            var connectedUserIds = sessions.GroupBy(e => e.User.Id).Select(e => e.Key);
            var notConnectedUserIds = userIds.Except(connectedUserIds);

            var bytes = SerializeObject(chatMessageBody);
            var userSessionsDeliveryMessage = await SendMessageToConnectedUsers(sessions, bytes, messageType);
            await DeliverMessageToDisconnectedUsers(notConnectedUserIds, userSessionsDeliveryMessage, bytes);
        }

        private async Task<MemoryStream?> ReceiveMessage(WebSocket webSocket, CancellationToken token)
        {
            byte[] bytes = new byte[4096];
            MemoryStream stream = new();

            WebSocketReceiveResult? receiveResult;
            do
            {
                receiveResult = await webSocket.ReceiveAsync(bytes, token);
                if (receiveResult.MessageType == WebSocketMessageType.Close && webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                    return null;
                }
                else if (receiveResult.Count > 0)
                    stream.Write(bytes, 0, receiveResult.Count);
            } while (!receiveResult.EndOfMessage && webSocket.State == WebSocketState.Open);

            return stream;
        }

        private async Task<bool> SendMessageToSession(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                await webSocket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return false;
            }

            return true;
        }

        private async Task<UserSessions?> NotifySessions(byte[] bytes, UserSessions userSessions)
        {
            var sessionsNotReceiveMessage = await _notificationService.SendMessageToSessions(userSessions.UserId, userSessions.SessionIds, bytes);
            return sessionsNotReceiveMessage.Any() ? new UserSessions
            {
                UserId = userSessions.UserId,
                SessionIds = sessionsNotReceiveMessage.ToList()
            } : null;
        }

        private async Task<IEnumerable<UserSessions>> SendMessageToConnectedUsers(IEnumerable<TaskChatSession> sessions, byte[] bytes, WebSocketMessageType messageType)
        {
            var userSessionsDeliveryMessage = new List<UserSessions>();

            foreach (var groupedSessions in sessions.GroupBy(e => e.User.Id))
            {
                var sessionsReceivedMessage = new List<Guid>();
                foreach (var session in groupedSessions)
                {
                    if (await SendMessageToSession(session.Ws, bytes, messageType))
                        sessionsReceivedMessage.Add(session.SessionId);
                }

                if (sessionsReceivedMessage.Any())
                    userSessionsDeliveryMessage.Add(new UserSessions { UserId = groupedSessions.Key, SessionIds = sessionsReceivedMessage });
            }

            return userSessionsDeliveryMessage;
        }

        private async Task<IEnumerable<UserSessions>> DeliverMessageToDisconnectedUsers(IEnumerable<Guid> userIds, IEnumerable<UserSessions> userSessions, byte[] bytes)
        {
            var userSessionsDeliveryMessage = new List<UserSessions>();

            foreach (var userId in userIds)
                await _notificationService.SendMessageToAllUserSessions(userId, bytes);

            foreach (var userSession in userSessions)
            {
                var userSessionDeliveryMessage = await NotifySessions(bytes, userSession);
                if (userSessionDeliveryMessage != null)
                    userSessionsDeliveryMessage.Add(userSessionDeliveryMessage);
            }

            return userSessionsDeliveryMessage;
        }

        private async Task<DateTime?> ProcessWebSocketState(
            WebSocket ws,
            IEnumerable<TaskChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            TaskChat chat,
            UserModel user,
            CancellationToken cancellationToken
        )
        {
            DateTime? dateLastViewingMessage = null;


            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, cancellationToken);
                    if (stream == null)
                        return dateLastViewingMessage;

                    dateLastViewingMessage = await ProcessReceivedMessage(stream, sessions, allUserIds, chat, user);
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogInformation($"{e.Message} by {user.Identifier}");
            }
            catch (WebSocketException e)
            {
                _logger.LogInformation($"{e.Message} by {user.Identifier}");
            }

            return dateLastViewingMessage;
        }

        private async Task<DateTime?> ProcessSentMessage(
            SentMessage sentMessage,
            IEnumerable<TaskChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            TaskChat chat,
            UserModel user
        )
        {
            if (sentMessage.LastMessageReadId == null)
            {
                var messageBody = sentMessage.MessageBody;
                if (messageBody.Type == MessageType.File && Guid.TryParse(messageBody.Content, out var messageId))
                {
                    var message = await _chatRepository.GetMessageAsync(messageId);
                    if (message != null)
                    {
                        await SendMessage(sessions, message.ToMessageBody(), WebSocketMessageType.Text, allUserIds, chat);
                        return message.SentAt;
                    }
                }
                else
                {
                    var chatMessage = await _chatRepository.AddMessageAsync(messageBody, chat, user);
                    await SendMessage(sessions, chatMessage.ToMessageBody(), WebSocketMessageType.Text, allUserIds, chat);
                    return chatMessage.SentAt;
                }
            }

            var lastMessage = await _chatRepository.GetMessageAsync((Guid)sentMessage.LastMessageReadId);
            return lastMessage?.SentAt;
        }

        private async Task<DateTime?> ProcessReceivedMessage(
            MemoryStream stream,
            IEnumerable<TaskChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            TaskChat chat,
            UserModel user
        )
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.GetBuffer();
            var input = Encoding.UTF8.GetString(bytes);

            if (input == null)
                return null;

            var sentMessage = JsonConvert.DeserializeObject<SentMessage>(input);

            return await ProcessSentMessage(sentMessage, sessions, allUserIds, chat, user);
        }

        private async Task UpdateLastViewingDate(TaskChatMembership chatMembership, UserTaskChatSession userChatSession, DateTime? dateLastViewingMessage)
        {
            if (dateLastViewingMessage != null)
            {
                var dateLastViewing = (DateTime)dateLastViewingMessage;
                await _chatRepository.UpdateLastViewingChatMembership(chatMembership, dateLastViewing);
                await _chatRepository.UpdateLastViewingUserChatSession(userChatSession, dateLastViewing);
            }
        }

        private async Task CloseWebSocket(WebSocket ws, string? errorMessage)
        {
            if (ws.State == WebSocketState.Open)
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, errorMessage, CancellationToken.None);
        }

        private byte[] SerializeObject<T>(T obj)
        {
            var serializableString = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(serializableString);
            return bytes;
        }
    }
}
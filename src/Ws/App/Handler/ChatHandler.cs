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
    public class ChatHandler : IChatHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<TaskChatHandler> _logger;
        private readonly IMainMonitoringService _monitoringService;

        public ChatHandler(
            IChatRepository chatRepository,
            ILogger<TaskChatHandler> logger,
            IMainMonitoringService monitoringService
        )
        {
            _chatRepository = chatRepository;
            _monitoringService = monitoringService;
            _logger = logger;
        }

        public async Task Invoke(
            UserModel user,
            ChatMembership chatMembership,
            Chat chat,
            ChatLobby lobby,
            ChatSession currentSession
        )
        {
            await Loop(lobby, currentSession, user, chat, chatMembership);
        }

        private async Task Loop(
            ChatLobby lobby,
            ChatSession currentSession,
            UserModel user,
            Chat chat,
            ChatMembership chatMembership
        )
        {
            var ws = currentSession.Ws;
            string? errorMessage = null;

            try
            {
                var connections = lobby.ActiveConnections;
                var chatUserIds = lobby.ChatUsers;

                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, CancellationToken.None);
                    if (stream == null)
                        return;


                    stream.Seek(0, SeekOrigin.Begin);
                    var bytes = stream.GetBuffer();
                    var otherConnections = connections.Where(e => e.User.Id != currentSession.User.Id);
                    var input = Encoding.UTF8.GetString(bytes);

                    if (input != null)
                    {
                        var messageBody = JsonConvert.DeserializeObject<CreateMessageBody>(input);

                        var userIds = chatUserIds.Where(userId => userId != user.Id);
                        if (messageBody.Type == MessageType.File && Guid.TryParse(messageBody.Content, out var messageId))
                        {
                            var message = await _chatRepository.GetMessageAsync(messageId);
                            if (message != null)
                                await SendMessageToAll(connections, message.ToMessageBody(), WebSocketMessageType.Text, userIds, chat);
                        }
                        else
                        {
                            var chatMessage = await _chatRepository.AddMessageAsync(messageBody, chat, user);
                            await SendMessageToAll(otherConnections, chatMessage.ToMessageBody(), WebSocketMessageType.Text, userIds, chat);
                        }
                    }
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogInformation($"{e.Message} by {currentSession.User.Email}");
                errorMessage = e.Message;
            }
            catch (WebSocketException e)
            {
                _logger.LogInformation($"{e.Message} by {currentSession.User.Email}");
                errorMessage = e.Message;
            }
            finally
            {
                if (ws.State == WebSocketState.Open)
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, errorMessage, CancellationToken.None);

                await _chatRepository.UpdateLastViewingChatMembership(chatMembership, DateTime.UtcNow);
            }
        }


        public async Task SendMessageToAll(
            IEnumerable<ChatSession> connections,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> userIds,
            Chat chat
        )
        {
            var temp = connections.Select(e => e.User.Id);
            var notConnectedUsers = userIds.Except(temp);
            var bytes = SerializeObject(message);

            var sendMessageTask = SendMessageToMainMonitoringService(message, notConnectedUsers, chat);

            foreach (var connection in connections)
                await SendMessage(connection.Ws, bytes, messageType);

            await sendMessageTask;
        }

        private async Task SendMessageToMainMonitoringService(MessageBody message, IEnumerable<Guid> userIds, Chat chat)
        {
            var chatMessageInfo = new ChatMessageInfo
            {
                ChatId = chat.Id,
                ChatType = ChatType.Personal,
                Message = message
            };

            foreach (var userId in userIds)
                await _monitoringService.SendMessage(userId, chatMessageInfo);
        }

        private async Task SendMessage(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                await webSocket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
            }
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

        private byte[] SerializeObject<T>(T obj)
        {
            var serializableString = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(serializableString);
            return bytes;
        }
    }
}

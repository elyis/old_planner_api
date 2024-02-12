using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.App.IHandler;
using old_planner_api.src.Ws.Entities;

namespace old_planner_api.src.Ws.App.Handler
{
    public class TaskChatHandler : ITaskChatHandler
    {
        private readonly ITaskChatRepository _chatRepository;
        private readonly ILogger<TaskChatHandler> _logger;

        public TaskChatHandler(
            ITaskChatRepository chatRepository,
            ILogger<TaskChatHandler> logger
        )
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task Invoke(
            UserModel user,
            TaskChatMembership userChatHistory,
            TaskChat chat,
            List<TaskChatSession> connections,
            TaskChatSession currentConnection
        )
        {
            await SendLastMessages(userChatHistory, userChatHistory.DateLastViewing, int.MaxValue, currentConnection.Ws);
            await Loop(connections, currentConnection, user, chat);
        }

        private async Task SendLastMessages(TaskChatMembership chatHistory, DateTime startedDate, int count, WebSocket ws)
        {
            var lastMessages = await _chatRepository.GetLastMessages(chatHistory, startedDate, count);
            var messages = lastMessages.Select(e => SerializeObject(e.ToMessageBody()));
            foreach (var message in messages)
                await ws.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Loop(
            List<TaskChatSession> allConnections,
            TaskChatSession currentConnection,
            UserModel user,
            TaskChat chat
        )
        {
            var ws = currentConnection.Ws;
            string? errorMessage = null;

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, CancellationToken.None);
                    if (stream == null)
                        return;


                    stream.Seek(0, SeekOrigin.Begin);
                    var bytes = stream.GetBuffer();
                    var otherConnections = allConnections.Where(e => e.User.Id != currentConnection.User.Id);
                    var input = Encoding.UTF8.GetString(bytes);

                    if (input != null)
                    {
                        var messageBody = JsonConvert.DeserializeObject<CreateMessageBody>(input);

                        if (messageBody.Type == MessageType.File && Guid.TryParse(messageBody.Content, out var messageId))
                        {
                            var message = await _chatRepository.GetMessageAsync(messageId);
                            if (message != null)
                            {
                                var serializeString = SerializeObject(message.ToMessageBody());
                                var tempBytes = Encoding.UTF8.GetBytes(serializeString);
                                await SendMessageToAll(allConnections, tempBytes, WebSocketMessageType.Text);
                            }
                        }
                        else
                        {
                            var chatMessage = await _chatRepository.AddAsync(messageBody, chat, user);
                            var serializeString = SerializeObject(chatMessage.ToMessageBody());
                            var temp = Encoding.UTF8.GetBytes(serializeString);
                            await SendMessageToAll(otherConnections, temp, WebSocketMessageType.Text);
                        }
                    }
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogInformation($"{e.Message} by {currentConnection.User.Email}");
                errorMessage = e.Message;
            }
            catch (WebSocketException e)
            {
                _logger.LogInformation($"{e.Message} by {currentConnection.User.Email}");
                errorMessage = e.Message;
            }
            finally
            {
                if (ws.State == WebSocketState.Open)
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, errorMessage, CancellationToken.None);
            }
        }


        public async Task SendMessageToAll(IEnumerable<TaskChatSession> connections, byte[] bytes, WebSocketMessageType messageType)
        {
            foreach (var connection in connections)
                await SendMessage(connection.Ws, bytes, messageType);
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

        private string SerializeObject<T>(T obj) => JsonConvert.SerializeObject(obj);
    }
}
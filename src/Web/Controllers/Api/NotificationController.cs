using System.Net;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers.Api
{
    [ApiController]
    [Route("")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IJwtService _jwtService;

        public NotificationController(
            INotificationService notificationService,
            IJwtService jwtService
        )
        {
            _jwtService = jwtService;
            _notificationService = notificationService;
        }

        [HttpGet("notify")]
        [SwaggerOperation("Подписаться на уведомления")]

        public async Task ConnectToMain(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return;

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var mainMonitoring = new UserNotificationSession
            {
                Socket = ws
            };

            _notificationService.AddConnection(tokenInfo.UserId, mainMonitoring);

            await Loop(ws);

            _notificationService.RemoveConnection(tokenInfo.UserId);
        }


        private async Task Loop(WebSocket ws)
        {
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, CancellationToken.None);
                    if (stream == null || stream.Length == 0)
                        return;
                }
            }
            catch (WebSocketException e)
            {
            }
            finally
            {
                if (ws.State == WebSocketState.Open)
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
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
    }
}
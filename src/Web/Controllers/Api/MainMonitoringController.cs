using System.Net;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("")]
    public class MainMonitoringController : ControllerBase
    {
        private readonly IMainMonitoringService _monitoringService;
        private readonly IJwtService _jwtService;


        public MainMonitoringController(
            IMainMonitoringService monitoringService,
            IJwtService jwtService)
        {
            _monitoringService = monitoringService;
            _jwtService = jwtService;
        }

        [HttpGet("main")]
        [SwaggerOperation("Подключиться к главному сокету")]

        public async Task ConnectToMain(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return;

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var mainMonitoring = new MainMonitoringSession
            {
                Socket = ws
            };

            _monitoringService.AddConnection(tokenInfo.UserId, mainMonitoring);

            await Loop(ws);

            _monitoringService.RemoveConnection(tokenInfo.UserId);
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
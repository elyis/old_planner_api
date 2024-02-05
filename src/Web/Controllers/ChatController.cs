using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.WebSockets.App.IHandler;
using old_planner_api.src.WebSockets.App.IService;
using old_planner_api.src.WebSockets.Entities;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly ITaskChatService _taskChatService;
        private readonly ITaskChatHandler _taskChatHandler;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ContentInspector _contentInspector;



        public ChatController(
            IJwtService jwtService,
            ITaskChatService taskChatService,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            ITaskChatHandler taskChatHandler,
            IFileUploaderService fileUploaderService,
            ContentInspector contentInspector
        )
        {
            _jwtService = jwtService;
            _taskChatService = taskChatService;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _taskChatHandler = taskChatHandler;
            _fileUploaderService = fileUploaderService;
            _contentInspector = contentInspector;
        }


        [HttpGet("chat"), Authorize]

        public async Task TaskChat(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] Guid chatId
        )
        {
            var websocketManager = HttpContext.WebSockets;
            if (!websocketManager.IsWebSocketRequest)
                return;

            var chat = await _chatRepository.GetAsync(chatId);
            if (chat == null)
                return;

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var ws = await websocketManager.AcceptWebSocketAsync();


            var userConnection = new UserTaskChatConnection
            {
                User = user.ToChatUserInfo(),
                Ws = ws
            };

            var userChatHistory = await _chatRepository.CreateOrGetUserChatHistoryAsync(chat, user);
            if (userChatHistory == null)
                return;

            var connections = _taskChatService.AddConnection(chatId, userConnection);
            await _taskChatHandler.Invoke(user, userChatHistory, chat, connections, userConnection);
            _taskChatService.RemoveConnection(chatId, userConnection);
        }


        [HttpPost("upload/chat/tasks"), Authorize]
        [SwaggerOperation("Загрузить файл в чат задачи")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(Guid))]

        public async Task<IActionResult> UploadChatAttachment(
                [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
                [Required, FromQuery] Guid chatId,
                [FromForm, Required] IFormFile file
            )
        {
            var resultUpload = await UploadFileAsync(Constants.localPathToTaskChatAttachments, file);
            if (resultUpload is not OkObjectResult)
                return resultUpload;


            var result = (OkObjectResult)resultUpload;
            var filename = (string)result.Value;
            var tokenInfo = _jwtService.GetTokenInfo(token);

            var chat = await _chatRepository.GetAsync(chatId);
            if (chat == null)
                return BadRequest();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var messageBody = new CreateMessageBody
            {
                Type = MessageType.File,
                Content = filename,
            };

            var chatMessage = await _chatRepository.AddAsync(messageBody, chat, user);
            if (chatMessage == null)
                return BadRequest();

            var serializableString = JsonConvert.SerializeObject(chatMessage.ToCreateMessageBody());
            var bytes = Encoding.UTF8.GetBytes(serializableString);

            var connections = _taskChatService.GetConnections(chatId);
            await _taskChatHandler.SendMessageToAll(connections, bytes, WebSocketMessageType.Text);
            return Ok();
        }

        private async Task<IActionResult> UploadFileAsync(string path, IFormFile file)
        {
            if (file == null)
                return BadRequest("No file uploaded");

            var stream = file.OpenReadStream();
            var mimeTypes = _contentInspector.Inspect(stream).ByFileExtension();
            var fileExtension = mimeTypes.MaxBy(e => e.Points)?.Extension ?? "txt";

            string? filename = await _fileUploaderService.UploadFileAsync(path, stream, $".{fileExtension}");
            if (filename == null)
                return BadRequest("Failed to upload the file");

            return Ok(filename);
        }
    }
}
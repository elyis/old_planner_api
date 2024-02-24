using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Entities.Shared;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.App.IHandler;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly IChatService _chatService;
        private readonly IChatHandler _chatHandler;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ContentInspector _contentInspector;


        public ChatController(
            IJwtService jwtService,
            IChatService taskChatService,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            IChatHandler taskChatHandler,
            IFileUploaderService fileUploaderService,
            ContentInspector contentInspector
        )
        {
            _jwtService = jwtService;
            _chatService = taskChatService;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _chatHandler = taskChatHandler;
            _fileUploaderService = fileUploaderService;
            _contentInspector = contentInspector;
        }


        [HttpGet("chat"), Authorize]
        [SwaggerOperation("Подключиться к чату")]
        public async Task ConnectToChat(
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


            var session = new ChatSession
            {
                User = user.ToChatUserInfo(),
                Ws = ws
            };

            var chatMembership = await _chatRepository.CreateOrGetChatMembershipAsync(chat, user);
            if (chatMembership == null)
                return;

            var chatMemberships = await _chatRepository.GetChatMembershipsAsync(chatId);
            var userIds = chatMemberships.Select(e => e.UserId).ToList();

            var connections = _chatService.AddConnection(chatId, session, userIds);
            await _chatHandler.Invoke(user, chatMembership, chat, connections, session);
            _chatService.RemoveConnection(chatId, session);
        }


        [HttpGet("api/chats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var chats = await _chatRepository.GetUserChats(tokenInfo.UserId);
            return Ok(chats);
        }

        [HttpPost("api/chat"), Authorize]
        [SwaggerOperation("Создать личный чат")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> CreatePersonalChat(
            [FromBody, Required] CreateChatBody chatBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, EmailAddress, Required] string email
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var addedUser = await _userRepository.GetAsync(email);
            if (addedUser == null)
                return NotFound("user not found");

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var participants = new List<UserModel>
            {
                user,
                addedUser
            };

            var result = await _chatRepository.AddPersonalChatAsync(participants, chatBody);
            return result != null ? Ok(result.Id) : Conflict();
        }

        [HttpPost("api/upload/chat"), Authorize]
        [SwaggerOperation("Загрузить файл в чат")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(Guid))]

        public async Task<IActionResult> UploadChatAttachment(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [Required, FromQuery] Guid chatId,
            [FromForm, Required] IFormFile file
        )
        {
            var resultUpload = await UploadFileAsync(Constants.localPathToPrivateChatAttachments, file);
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

            var chatMessage = await _chatRepository.AddMessageAsync(messageBody, chat, user);
            if (chatMessage == null)
                return BadRequest();

            var chatMemberships = await _chatRepository.GetChatMembershipsAsync(chatId);
            var userIds = chatMemberships.Select(e => e.UserId).ToList();

            var lobby = _chatService.GetConnections(chatId);
            if (lobby != null)
                await _chatHandler.SendMessageToAll(lobby.ActiveConnections, chatMessage.ToMessageBody(), WebSocketMessageType.Text, userIds, chat);
            return Ok();
        }

        [HttpGet("api/upload/chat/{filename}")]
        [SwaggerOperation("Получить файл чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetPrivateChatAttachments([Required] string filename)
            => await GetIconAsync(Constants.localPathToPrivateChatAttachments, filename);

        [HttpPost("api/upload/chatIcon"), Authorize]
        [SwaggerOperation("Загрузить иконку чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(Guid))]

        public async Task<IActionResult> UploadChatIcon(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [Required, FromQuery] Guid chatId,
            [FromForm, Required] IFormFile file
        )
        {
            var resultUpload = await UploadFileAsync(Constants.localPathToChatIcons, file);
            if (resultUpload is not OkObjectResult)
                return resultUpload;


            var result = (OkObjectResult)resultUpload;
            var filename = (string)result.Value;

            var chat = await _chatRepository.UpdateChatImage(chatId, filename);
            return chat == null ? BadRequest() : Ok();
        }

        [HttpPost("api/chat/messages")]
        [SwaggerOperation("Получить список сообщений")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetMessages(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid chatId,
            [FromBody, Required] DynamicDataLoadingOptions loadingOptions
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var membership = await _chatRepository.GetMembershipAsync(chatId, tokenInfo.UserId);
            if (membership == null)
                return Forbid();

            var messages = await _chatRepository.GetMessagesAsync(chatId, loadingOptions.Count, loadingOptions.LoadPosition);

            var result = messages.Select(e => e.ToMessageBody());
            return Ok(result);
        }


        [HttpGet("api/upload/chatIcon/{filename}")]
        [SwaggerOperation("Получить иконку чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetChatIcon([Required] string filename)
            => await GetIconAsync(Constants.localPathToChatIcons, filename);

        private async Task<IActionResult> UploadFileAsync(string path, IFormFile file)
        {
            if (file == null)
                return BadRequest("No file uploaded");

            string? fileExtension = file.FileName.Split(".").LastOrDefault();
            var stream = file.OpenReadStream();

            if (fileExtension == null)
            {
                var mimeTypes = _contentInspector.Inspect(stream).ByFileExtension();
                fileExtension = mimeTypes.MaxBy(e => e.Points)?.Extension ?? "txt";
            }

            string? filename = await _fileUploaderService.UploadFileAsync(path, stream, $".{fileExtension}");
            if (filename == null)
                return BadRequest("Failed to upload the file");

            return Ok(filename);
        }

        private async Task<IActionResult> GetIconAsync(string path, string filename)
        {
            var bytes = await _fileUploaderService.GetStreamFileAsync(path, filename);
            if (bytes == null)
                return NotFound();

            var fileExtension = Path.GetExtension(filename);
            return File(bytes, $"image/{fileExtension}", filename);
        }
    }
}

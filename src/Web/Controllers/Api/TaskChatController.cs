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
using old_planner_api.src.Ws.App.IHandler;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.Entities;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;


namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    public class TaskChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly ITaskChatConnectionService _chatConnectionService;
        private readonly INotificationService _notificationService;
        private readonly IChatHandler _chatHandler;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ContentInspector _contentInspector;


        public TaskChatController(
            IJwtService jwtService,
            ITaskChatConnectionService taskChatService,
            INotificationService notificationService,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            IChatHandler taskChatHandler,
            IFileUploaderService fileUploaderService,
            ContentInspector contentInspector
        )
        {
            _jwtService = jwtService;
            _chatConnectionService = taskChatService;
            _notificationService = notificationService;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _chatHandler = taskChatHandler;
            _fileUploaderService = fileUploaderService;
            _contentInspector = contentInspector;
        }


        [HttpGet("taskChat"), Authorize]
        [SwaggerOperation("Подключиться к чату")]

        public async Task ConnectToTaskChat(
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
                Ws = ws,
                SessionId = tokenInfo.SessionId
            };

            var chatMembership = await _chatRepository.CreateOrGetChatMembershipAsync(chat, user);
            if (chatMembership == null)
                return;

            var userChatSession = await _chatRepository.GetUserChatSessionAsync(tokenInfo.SessionId, chatMembership.Id);
            if (userChatSession == null)
                return;

            if (!_chatConnectionService.LobbyIsExist(chatId))
            {
                var chatMemberships = await _chatRepository.GetChatMembershipsAsync(chatId);
                var userIds = chatMemberships.Select(e => e.UserId).ToList();
                _chatConnectionService.AddLobby(chatId, userIds);
            }

            var lobby = _chatConnectionService.AddSessionToLobby(chatId, session);
            if (lobby == null)
                return;

            await _chatHandler.Invoke(user, chatMembership, chat, lobby, session, userChatSession);
            _chatConnectionService.RemoveConnection(chatId, session);
        }

        [HttpPost("api/taskChat/messages")]
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


        [HttpGet("api/taskChats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var result = await _chatRepository.GetChatBodies(tokenInfo.UserId, tokenInfo.SessionId, ChatType.Task);
            return Ok(result);
        }

        [HttpPost("api/taskChat"), Authorize]
        [SwaggerOperation("Добавить пользователя к чату задачи")]
        [SwaggerResponse(200, Description = "Возвращаются идентификаторы не добавленных пользователей", Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddUsersToTaskChatMembership(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] List<string> identifiers,
            [FromHeader, Required] Guid chatId
        )
        {

            var userIdentifiers = identifiers.ToHashSet().ToList();
            var users = await _userRepository.GetUsersAsync(userIdentifiers);

            var chat = await _chatRepository.GetAsync(chatId);
            if (chat == null)
                return BadRequest("chatId is empty");

            if (users.Count != userIdentifiers.Count)
                return BadRequest();

            var tokenPayload = _jwtService.GetTokenInfo(token);
            var taskCreator = await _userRepository.GetAsync(tokenPayload.UserId);
            if (taskCreator == null)
                return Unauthorized();

            var notAddedUsers = new List<string>();
            foreach (var user in users)
            {
                var userSessions = await _userRepository.GetUserSessionsAsync(user.Id);
                var result = await _chatRepository.AddMembershipAsync(user, chat);
                if (result == null)
                    notAddedUsers.Add(user.Identifier);
                else
                    await _chatRepository.CreateUserChatSessionAsync(userSessions, result, result.DateLastViewing);

                var message = new CreateMessageBody
                {
                    Content = "Вы были выбраны исполнителем задачи",
                    Type = MessageType.Text
                };

                var chatMessage = await _chatRepository.AddMessageAsync(message, chat, taskCreator);
                var messageBody = chatMessage.ToMessageBody();

                var bytes = SerializeObject(messageBody);
                await _notificationService.SendMessageToAllUserSessions(user.Id, bytes);
            }

            return Ok(notAddedUsers);
        }

        [HttpPost("api/upload/taskChat"), Authorize]
        [SwaggerOperation("Загрузить файл в чат задачи")]
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

            var curMembership = await _chatRepository.GetMembershipAsync(chatId, tokenInfo.UserId);
            var userSession = await _chatRepository.GetUserChatSessionAsync(tokenInfo.SessionId, curMembership.Id);

            var memberships = await _chatRepository.GetChatMembershipsAsync(chatId);
            var userIds = memberships.Select(e => e.UserId).ToList();
            await _chatRepository.UpdateLastViewingChatMembership(curMembership, chatMessage.SentAt);
            await _chatRepository.UpdateLastViewingUserChatSession(userSession, chatMessage.SentAt);

            var chatLobby = _chatConnectionService.GetConnections(chatId);
            if (chatLobby != null)
                await _chatHandler.SendMessage(chatLobby.ActiveSessions.Select(e => e.Value), chatMessage.ToMessageBody(), WebSocketMessageType.Text, userIds, chat);
            return Ok();
        }


        [HttpGet("api/upload/taskChat/{filename}")]
        [SwaggerOperation("Получить файл чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetTaskChatAttachments([Required] string filename)
            => await GetIconAsync(Constants.localPathToChatIcons, filename);

        [HttpGet("api/upload/taskChatIcon/{filename}")]
        [SwaggerOperation("Получить иконку чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetChatIcon([Required] string filename)
            => await GetIconAsync(Constants.localPathToChatIcons, filename);

        [HttpPost("api/upload/taskChatIcon"), Authorize]
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

        private byte[] SerializeObject<T>(T obj)
        {
            var serializableString = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(serializableString);
            return bytes;
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using Newtonsoft.Json;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
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
        private readonly ITaskChatService _taskChatService;
        private readonly ITaskChatHandler _taskChatHandler;
        private readonly ITaskChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ContentInspector _contentInspector;


        public TaskChatController(
            IJwtService jwtService,
            ITaskChatService taskChatService,
            ITaskChatRepository chatRepository,
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

            var chat = await _chatRepository.GetChatAsync(chatId);
            if (chat == null)
                return;

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var ws = await websocketManager.AcceptWebSocketAsync();


            var userConnection = new TaskChatSession
            {
                User = user.ToChatUserInfo(),
                Ws = ws,
            };

            var currentChatMembership = await _chatRepository.AddOrGetChatMembershipAsync(chat, user);
            if (currentChatMembership == null)
                return;

            var chatMemberships = await _chatRepository.GetChatMembershipsAsync(chatId);
            var userIds = chatMemberships.Select(e => e.ParticipantId).ToList();

            var connections = _taskChatService.AddConnection(chatId, userConnection, userIds);
            await _taskChatHandler.Invoke(user, currentChatMembership, chat, connections, userConnection);
            _taskChatService.RemoveConnection(chatId, userConnection);
        }

        [HttpGet("api/taskChat/messages")]
        [SwaggerOperation("Получить список последних сообщений в чате")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]

        public async Task<IActionResult> GetMessages(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid chatId,
            [FromQuery, Range(0, int.MaxValue)] int count
        )
        {
            if (count == 0)
                return Ok(new List<MessageBody>());

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var chatMembership = await _chatRepository.GetTaskChatMembershipAsync(chatId, tokenInfo.UserId);
            if (chatMembership == null)
                return BadRequest();

            var messages = await _chatRepository.GetLastMessagesAndUpdateLastViewing(chatMembership, chatMembership.DateLastViewing, count);
            var result = messages.Select(e => e.ToMessageBody());
            return Ok(result);
        }


        [HttpGet("api/taskChats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<TaskChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var result = await _chatRepository.GetUserChatBodies(tokenInfo.UserId);
            return Ok(result);
        }

        [HttpPost("api/taskChat"), Authorize]
        [SwaggerOperation("Добавить пользователя к чату задачи")]
        [SwaggerResponse(200, Description = "Возвращаются идентификаторы не добавленных пользователей", Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddUsersToTaskChatMembership(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] List<string> userEmails,
            [FromHeader, Required] Guid chatId
        )
        {
            var userEmailsSet = userEmails.ToHashSet().ToList();
            var users = await _userRepository.GetUsersAsync(userEmailsSet);
            foreach (var userEmail in userEmailsSet)
            {
                if (!IsValidEmail(userEmail))
                    return BadRequest($"Invalid email format: {userEmail}");
            }


            var chat = await _chatRepository.GetChatAsync(chatId);
            if (chat == null)
                return BadRequest("chatId is empty");

            if (users.Count != userEmailsSet.Count)
                return BadRequest();

            var notAddedUsers = new List<string>();
            foreach (var user in users)
            {
                var result = await _chatRepository.AddChatMembershipAsync(chat, user);
                if (result == null)
                    notAddedUsers.Add(user.Email);
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
            var resultUpload = await UploadFileAsync(Constants.localPathToTaskChatAttachments, file);
            if (resultUpload is not OkObjectResult)
                return resultUpload;


            var result = (OkObjectResult)resultUpload;
            var filename = (string)result.Value;
            var tokenInfo = _jwtService.GetTokenInfo(token);

            var chat = await _chatRepository.GetChatAsync(chatId);
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

            var chatLobby = _taskChatService.GetConnections(chatId);
            if (chatLobby != null)
                await _taskChatHandler.SendMessageToAll(chatLobby.ActiveConnections, chatMessage.ToMessageBody(), WebSocketMessageType.Text, chatLobby.ChatUsers, chat);
            return Ok();
        }


        [HttpGet("api/upload/taskChat/{filename}")]
        [SwaggerOperation("Получить файл чата")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetTaskChatAttachments([Required] string filename)
            => await GetIconAsync(Constants.localPathToTaskChatAttachments, filename);

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


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
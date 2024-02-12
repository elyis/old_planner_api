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

            var userChatHistory = await _chatRepository.CreateOrGetChatMembershipAsync(chat, user);
            if (userChatHistory == null)
                return;

            var connections = _chatService.AddConnection(chatId, session);
            await _chatHandler.Invoke(user, userChatHistory, chat, connections, session);
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
            var chatMemberships = await _chatRepository.GetUserChatMemberships(tokenInfo.UserId);

            var result = chatMemberships
                .GroupBy(e => e.ChatId)
                .Select(group => new ChatBody
                {
                    Id = group.Key,
                    Participants = group.Select(e => e.User.ToChatUserInfo()).ToList()
                });

            return Ok(result);
        }

        [HttpPost("api/chat"), Authorize]
        [SwaggerOperation("Создать личный чат")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<Guid>))]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> CreatePersonalChat(
            [FromBody, Required] CreateChatBody chatBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, EmailAddress, Required] string email
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            if (chatBody.ParticipantIds.Contains(tokenInfo.UserId))
                chatBody.ParticipantIds.Remove(tokenInfo.UserId);

            if (!chatBody.ParticipantIds.Any())
                return BadRequest();


            if (chatBody.Type == ChatType.Personal)
            {
                if (chatBody.ParticipantIds.Count == 1)
                {

                }
                else
                    return BadRequest("Personal chat is limit 2 users");
            }
            else
                return BadRequest("Логика не реализована");


            var addedUser = await _userRepository.GetAsync(email);
            if (addedUser == null)
                return NotFound("user not found");

            var chatMembership = await _chatRepository.GetChatMembershipAsync(tokenInfo.UserId, addedUser.Id, ChatType.Personal);
            if (chatMembership != null)
                return Conflict();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);

            var createChatBody = new CreateChatBody
            {
                Name = addedUser.Email,
                Type = ChatType.Personal,
                ParticipantIds = new List<Guid>
                {
                    user.Id,
                    addedUser.Id
                }
            };

            var participants = new List<UserModel>
            {
                user,
                addedUser
            };

            var result = await _chatRepository.AddChatAsync(participants, createChatBody);

            return result != null ? Ok(result.Id) : BadRequest();
        }

        [HttpPost("api/chat"), Authorize]
        [SwaggerOperation("Создать чат")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<string>))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddUsersToChatMembership(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, EmailAddress, Required] string email,
            [FromHeader, Required] Guid? chatId
        )
        {
            var addedUser = await _userRepository.GetAsync(email);
            if (addedUser == null)
                return NotFound("user not found");

            if (chatId != null)
            {
                var chat = await _chatRepository.GetAsync((Guid)chatId);
                if (chat == null)
                    return BadRequest("chatId is empty");

                var countMemberships = await _chatRepository.GetCountChatMemberships(chat.Id);
                if (countMemberships >= 2 && chat.Type == ChatType.Personal.ToString())
                    return BadRequest("Personal chat is limited to two users");


                var chatMembership = await _chatRepository.GetMembershipAsync(chat.Id, addedUser.Id);
                if (chatMembership != null)
                    return Conflict("Chat is exist");

                chatMembership = await _chatRepository.AddAsync(chat, addedUser);
            }
            else
            {

            }
            return Ok();
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



        [HttpPost("api/upload/chat/tasks"), Authorize]
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

            var chatMessage = await _chatRepository.AddAsync(messageBody, chat, user);
            if (chatMessage == null)
                return BadRequest();

            var serializableString = JsonConvert.SerializeObject(chatMessage.ToCreateMessageBody());
            var bytes = Encoding.UTF8.GetBytes(serializableString);

            var connections = _chatService.GetConnections(chatId);
            await _chatHandler.SendMessageToAll(connections, bytes, WebSocketMessageType.Text);
            return Ok();
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
    }
}

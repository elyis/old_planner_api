using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Ws.App.IService;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class MessageController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        public MessageController(
            IEmailService emailService,
            INotificationService notificationService,
            IJwtService jwtService,
            IUserRepository userRepository,
            IChatRepository chatRepository
        )
        {
            _emailService = emailService;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _jwtService = jwtService;
        }

        [HttpPost("message/email"), Authorize]

        public async Task<IActionResult> SendMessageByEmail(
            SentMessageBody message,
            [FromQuery, EmailAddress] string email,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);

            var user = await _userRepository.GetAsync(email);
            if (user == null || user.Id == tokenInfo.UserId)
                return BadRequest();

            var currentUser = await _userRepository.GetAsync(tokenInfo.UserId);
            if (currentUser == null)
                return BadRequest();

            Task? task;
            if (currentUser.AuthenticationMethod == AuthenticationMethod.Email.ToString())
                task = _emailService.SendMessage(currentUser.Identifier, email, message.Subject, message.Content);
            else
                task = _emailService.SendMessage(email, message.Subject, message.Content);

            var chatMembership = await _chatRepository.GetPersonalChatAsync(user.Id, currentUser.Id);
            var newMessage = new CreateMessageBody
            {
                Content = message.Content,
                Type = MessageType.Text,
            };
            Chat? chat;
            if (chatMembership == null)
            {
                var participants = new List<UserModel>()
                {
                    user,
                    currentUser
                };
                var chatBody = new CreateChatBody
                {
                    Name = user.Nickname,
                };
                chat = await _chatRepository.AddPersonalChatAsync(participants, chatBody, DateTime.UtcNow);
            }
            else
                chat = await _chatRepository.GetAsync(chatMembership.ChatId);

            await _chatRepository.AddMessageAsync(newMessage, chat, currentUser);
            await task;

            var bytes = Encoding.UTF8.GetBytes(message.Content);
            await _notificationService.SendMessageToAllUserSessions(user.Id, bytes);

            return Ok();
        }
    }
}
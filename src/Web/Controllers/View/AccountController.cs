using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Provider;

namespace old_planner_api.src.Web.Controllers.View
{
    [Route("")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IGoogleTokenService _googleTokenService;
        private readonly IMailRuTokenService _mailRuTokenService;


        public AccountController(
            ILogger<AccountController> logger,
            IAuthService authService,
            IJwtService jwtService,
            IUserRepository userRepository,
            IEmailService emailService,
            IGoogleTokenService googleTokenService,
            IMailRuTokenService mailRuTokenService
        )
        {
            _logger = logger;
            _authService = authService;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _emailService = emailService;
            _googleTokenService = googleTokenService;
            _mailRuTokenService = mailRuTokenService;
        }

        [HttpGet("mail-login")]
        public IActionResult MailLogin([FromQuery(Name = "deviceId"), Required] string deviceId)
        {
            var authorizationUrl = _mailRuTokenService.GetAuthorizationUrl();
            var deviceTypeId = GetDeviceType(deviceId);
            if (deviceTypeId == null)
                return BadRequest();

            HttpContext.Session.SetString("deviceId", deviceId);
            HttpContext.Session.SetString("deviceTypeId", deviceTypeId.ToString());
            return Redirect(authorizationUrl);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery(Name = "deviceId"), Required] string deviceId)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items = { { "access_type", "offline" } }
            };


            var deviceTypeId = GetDeviceType(deviceId);
            if (deviceTypeId == null)
                return BadRequest();

            properties.SetParameter("access_type", "offline");

            HttpContext.Session.SetString("deviceId", deviceId);
            HttpContext.Session.SetString("deviceTypeId", deviceTypeId.ToString());

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("singin-google")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Unauthorized();

            var claims = result.Principal.Claims;
            var email = claims.First(e => e.Type == ClaimTypes.Email).Value;
            var name = claims.First(e => e.Type == ClaimTypes.Name).Value;
            var nameIdentifier = claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value;

            var session = HttpContext.Session;
            var deviceTypeId = session.GetString("deviceTypeId");
            var deviceId = session.GetString("deviceId");

            var access_token = result.Properties.GetTokenValue("access_token");
            var refreshToken = result.Properties.GetTokenValue("refresh_token");

            var isExist = await _authService.AccountIsExist(email);
            if (isExist)
            {
                var accountCreatedByGoogle = await _authService.AccountAuthorizedByProvider(email, AuthenticationProviderType.Google);
                if (!accountCreatedByGoogle)
                    return Conflict();

                var mailCreadentials = await _authService.CreateMailCredentials(email, access_token, refreshToken, EmailProvider.Gmail);
                var signInBody = new SignInBody
                {
                    Identifier = email,
                    Method = AuthenticationMethod.Email,
                    Password = Hmac512Provider.Compute(nameIdentifier),
                    DeviceId = deviceId,
                    DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
                };

                var signInResponse = await _authService.SignIn(signInBody, AuthenticationProviderType.Google);
                return signInResponse;
            }

            var signUpBody = new SignUpBody
            {
                Identifier = email,
                Method = AuthenticationMethod.Email,
                Nickname = name,
                Password = Hmac512Provider.Compute(nameIdentifier),
                DeviceId = deviceId,
                DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
            };

            var response = await _authService.SignUp(signUpBody, UserRole.Common.ToString(), AuthenticationProviderType.Google);
            if (response is OkObjectResult objectResult)
            {
                var mailCreadentials = await _authService.CreateMailCredentials(email, access_token, refreshToken, EmailProvider.Gmail);
            }
            return response;
        }

        [HttpGet("signin-mail")]
        public async Task<IActionResult> MailResponse(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code is missing.");

            var tokenPairResponse = await _mailRuTokenService.GetTokenAsync(code);
            if (tokenPairResponse == null)
                return BadRequest("Failed to retrieve access token.");

            var session = HttpContext.Session;
            var deviceTypeId = session.GetString("deviceTypeId");
            var deviceId = session.GetString("deviceId");

            var userInfo = await _mailRuTokenService.GetUserInfo(tokenPairResponse.AccessToken);
            if (userInfo == null)
                return BadRequest("Получить профиль пользователя не получается");

            var isAccountExist = await _authService.AccountIsExist(userInfo.Email);
            if (!isAccountExist)
            {
                var signUpBody = new SignUpBody
                {
                    Identifier = userInfo.Email,
                    Method = AuthenticationMethod.Email,
                    Nickname = userInfo.Nickname,
                    Password = Hmac512Provider.Compute(userInfo.FirstName),
                    DeviceId = deviceId,
                    DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
                };

                var response = await _authService.SignUp(signUpBody, UserRole.Common.ToString(), AuthenticationProviderType.MailRu);
                if (response is OkObjectResult objectResult)
                {
                    var mailCreadentials = await _authService.CreateMailCredentials(userInfo.Email, tokenPairResponse.AccessToken, tokenPairResponse.RefreshToken, EmailProvider.MailRu);
                }
                return response;
            }
            else
            {
                var isAccountCreatedByMailRu = await _authService.AccountAuthorizedByProvider(userInfo.Email, AuthenticationProviderType.MailRu);
                if (isAccountCreatedByMailRu)
                {
                    var signInBody = new SignInBody
                    {
                        Identifier = userInfo.Email,
                        Method = AuthenticationMethod.Email,
                        Password = Hmac512Provider.Compute(userInfo.FirstName),
                        DeviceId = deviceId,
                        DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
                    };

                    var signInResponse = await _authService.SignIn(signInBody, AuthenticationProviderType.MailRu);
                    return signInResponse;
                }

                return Conflict();
            }
        }

        [HttpGet("emails"), Authorize]
        [SwaggerOperation("Получить список сообщений с почты")]
        [SwaggerResponse(200, "Успешно", Type = typeof(List<EmailMessageInfo>))]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Почтовый аккаунт не подключен")]
        public async Task<IActionResult> GetEmails(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery] int offset = 0,
            [FromQuery] int count = 10,
            [FromQuery] EmailProvider emailProvider = EmailProvider.Gmail)
        {
            var userId = _jwtService.GetTokenInfo(token).UserId;
            var mailCreadentials = await _userRepository.GetUserMailCredentials(userId);
            string refreshToken = "";
            string accessToken = "";

            var emailCredentials = mailCreadentials.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return NotFound();

            switch (emailProvider)
            {
                case EmailProvider.Gmail:
                    var googleTokenResponse = await _googleTokenService.RefreshAccessTokenAsync(emailCredentials.RefreshToken);
                    refreshToken = googleTokenResponse.RefreshToken;
                    accessToken = googleTokenResponse.AccessToken;
                    break;

                case EmailProvider.MailRu:
                    var mailTokenPair = await _mailRuTokenService.UpdateToken(emailCredentials.RefreshToken);
                    if (mailTokenPair == null)
                        return BadRequest();

                    refreshToken = emailCredentials.RefreshToken;
                    accessToken = mailTokenPair.AccessToken;
                    break;
            }

            var messages = await _emailService.GetMessages(emailCredentials.Email, accessToken, refreshToken, offset, count, emailProvider);
            return Ok(messages);
        }

        [HttpDelete("emails"), Authorize]
        [SwaggerOperation("Удалить письма по индексам")]
        [SwaggerResponse(204, "Успешно")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Почтовый аккаунт не подключен")]

        public async Task<IActionResult> RemoveEmails(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery, Required] int[] messageIndexes,
            [FromQuery] EmailProvider emailProvider = EmailProvider.Gmail)
        {
            var userId = _jwtService.GetTokenInfo(token).UserId;
            var mailCreadentials = await _userRepository.GetUserMailCredentials(userId);
            string refreshToken = "";
            string accessToken = "";

            var emailCredentials = mailCreadentials.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return NotFound();

            switch (emailProvider)
            {
                case EmailProvider.Gmail:
                    var googleTokenResponse = await _googleTokenService.RefreshAccessTokenAsync(emailCredentials.RefreshToken);
                    refreshToken = googleTokenResponse.RefreshToken;
                    accessToken = googleTokenResponse.AccessToken;
                    break;

                case EmailProvider.MailRu:
                    var mailTokenPair = await _mailRuTokenService.UpdateToken(emailCredentials.RefreshToken);
                    if (mailTokenPair == null)
                        return BadRequest();

                    refreshToken = emailCredentials.RefreshToken;
                    accessToken = mailTokenPair.AccessToken;
                    break;
            }

            await _emailService.DeleteMessages(emailCredentials.Email, accessToken, new List<int>(messageIndexes), emailProvider);
            return NoContent();
        }

        [HttpPost("email"), Authorize]
        [SwaggerOperation("Отправить сообщение на почту")]
        [SwaggerResponse(200, "Успешно")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Нет подключенных ящиков")]
        [SwaggerResponse(415, "Неподдерживаемый формат почты")]
        public async Task<IActionResult> SendMessage(
            [FromHeader(Name = "Authorization")] string token,
            [FromBody] CreateEmailMessageBody body)
        {
            var userId = _jwtService.GetTokenInfo(token).UserId;
            var mailCreadentials = await _userRepository.GetUserMailCredentials(userId);

            var senderUser = await _userRepository.GetAsync(userId);
            var recipientUser = await _userRepository.GetAsync(body.ToEmail);
            if (recipientUser == null)
                return NotFound("Почта получателя не найдена");

            var emailCredentials = mailCreadentials.FirstOrDefault();
            if (emailCredentials == null)
                return NotFound("Нет подключенных почтовых ящиков");

            string password = emailCredentials.AccessToken;

            EmailProvider emailProvider = EmailProvider.Gmail;
            if (emailCredentials.EmailProvider == EmailProvider.Gmail.ToString())
            {
                var googleTokenResponse = await _googleTokenService.RefreshAccessTokenAsync(emailCredentials.RefreshToken);
                password = googleTokenResponse.AccessToken;
            }
            else if (emailCredentials.EmailProvider == EmailProvider.MailRu.ToString())
            {
                var mailTokenPair = await _mailRuTokenService.UpdateToken(emailCredentials.RefreshToken);
                if (mailTokenPair == null)
                    return BadRequest();

                password = mailTokenPair.AccessToken;
                emailProvider = EmailProvider.MailRu;
            }

            else
                return new UnsupportedMediaTypeResult();

            await _emailService.SendMessage(emailCredentials.Email, senderUser.Nickname, body.ToEmail, recipientUser.Nickname, body.Subject, body.Message, password, emailProvider);
            return Ok();
        }

        private DeviceTypeId? GetDeviceType(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            var regex = new Regex("^[0-9a-fA-F]{16}$");
            if (regex.IsMatch(deviceId))
                return DeviceTypeId.AndroidId;
            if (Guid.TryParse(deviceId, out _))
                return DeviceTypeId.UUID;

            return null;
        }
    }
}
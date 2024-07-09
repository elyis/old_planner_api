using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Shared;
using old_planner_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Provider;
using webApiTemplate.src.Domain.Entities.Shared;
using old_planner_api.src.Domain.Enums;
using System.Text.RegularExpressions;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IJwtService _jwtService;

        public AuthService
        (
            IUserRepository userRepository,
            IChatRepository chatRepository,
            IJwtService jwtService
        )
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> RestoreToken(string refreshToken, string deviceId, DeviceTypeId deviceTypeId)
        {
            var isValidDeviceId = IsValidDeviceId(deviceId, deviceTypeId);
            if (!isValidDeviceId)
                return new BadRequestObjectResult("DeviceId is not correct format");

            var session = await _userRepository.GetUserSessionByTokenAndUser(refreshToken);
            if (session == null)
                return new NotFoundResult();

            var user = session.User;
            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> SignIn(SignInBody body, AuthenticationProviderType provider)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new BadRequestResult();

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new BadRequestObjectResult("DeviceId is not correct format");

            var user = await _userRepository.GetAsync(body.Identifier);
            if (user == null)
                return new NotFoundResult();

            var inputPasswordHash = Hmac512Provider.Compute(body.Password);
            if (user.PasswordHash != inputPasswordHash)
                return new BadRequestResult();

            var session = await CreateChatMemberships(body.DeviceId, user);
            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> CreateMailCredentials(string email, string access_token, string refresh_token, EmailProvider provider)
        {
            var user = await _userRepository.GetAsync(email);
            if (user == null)
                return new NotFoundResult();

            var result = await _userRepository.AddUserMailCredential(email, access_token, refresh_token, user, provider);
            return result == null ? new ConflictResult() : new OkResult();
        }

        public async Task<IActionResult> SignUp(SignUpBody body, string rolename, AuthenticationProviderType provider)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new BadRequestResult();

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new BadRequestObjectResult("DeviceId is not correct format");

            var user = await _userRepository.AddAsync(body, rolename, provider);
            if (user == null)
                return new ConflictResult();

            var session = await CreateChatMemberships(body.DeviceId, user);
            var tokenPair = await UpdateToken(rolename, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        private async Task<UserSession> CreateChatMemberships(string deviceId, UserModel user)
        {
            var session = await _userRepository.AddUserSessionAsync(deviceId, user);
            if (session == null)
            {
                session = await _userRepository.GetSessionAsync(user.Id, deviceId);
                return session;
            }

            await _chatRepository.CreateUserChatSessionsAsync(session);

            return session;
        }

        private async Task<TokenPair> UpdateToken(string rolename, Guid userId, Guid sessionId)
        {
            var tokenInfo = new TokenInfo
            {
                Role = rolename,
                UserId = userId,
                SessionId = sessionId
            };

            var tokenPair = _jwtService.GenerateDefaultTokenPair(tokenInfo);
            tokenPair.RefreshToken = await _userRepository.UpdateTokenAsync(tokenPair.RefreshToken, tokenInfo.SessionId);
            return tokenPair;
        }

        private bool IsValidDeviceId(string deviceId, DeviceTypeId deviceTypeId)
        {
            switch (deviceTypeId)
            {
                case DeviceTypeId.AndroidId:
                    var regex = new Regex("^[0-9a-fA-F]{16}$");
                    return regex.IsMatch(deviceId);

                case DeviceTypeId.UUID:
                    return Guid.TryParse(deviceId, out var uuid);
            }

            return false;
        }

        private bool IsValidAuthenticationIdentifier(string identifier, AuthenticationMethod method)
        {
            return method switch
            {
                AuthenticationMethod.Email => IsValidEmail(identifier),
                AuthenticationMethod.Phone => IsValidPhoneNumber(identifier),
                _ => false,
            };
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            var phoneRegex = new Regex(@"^\+?\d+$");
            return phoneRegex.IsMatch(phoneNumber);
        }

        public async Task<bool> AccountIsExist(string identifier)
        {
            var user = await _userRepository.GetAsync(identifier);
            return user != null;
        }

        public async Task<bool> AccountAuthorizedByProvider(string identifier, AuthenticationProviderType provider)
        {
            var user = await _userRepository.GetAsync(identifier);
            return user != null && user.AuthorizationProvider == provider.ToString();
        }
    }
}
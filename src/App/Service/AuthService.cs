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

namespace old_planner_api.src.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService
        (
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> RestoreToken(string refreshToken, string deviceId, DeviceTypeId deviceTypeId)
        {
            var isValidDeviceId = IsValidDeviceId(deviceId, deviceTypeId);
            if (!isValidDeviceId)
                return new BadRequestObjectResult("DeviceId is not correct format");

            var user = await _userRepository.GetByTokenAsync(refreshToken);
            if (user == null)
                return new NotFoundResult();

            var session = await _userRepository.GetSessionAsync(user.Id, deviceId);
            if (session == null)
                return new ForbidResult();

            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> SignIn(SignInBody body)
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

            var session = await _userRepository.AddOrGetUserSessionAsync(body.DeviceId, user);
            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> SignUp(SignUpBody body, string rolename)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new BadRequestResult();

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new BadRequestObjectResult("DeviceId is not correct format");

            var user = await _userRepository.AddAsync(body, rolename);
            if (user == null)
                return new ConflictResult();

            var session = await _userRepository.AddOrGetUserSessionAsync(body.DeviceId, user);
            var tokenPair = await UpdateToken(rolename, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
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
            tokenPair.RefreshToken = await _userRepository.UpdateTokenAsync(tokenPair.RefreshToken, tokenInfo.UserId);
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
    }
}
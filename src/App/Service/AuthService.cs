using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Shared;
using old_planner_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Provider;
using webApiTemplate.src.Domain.Entities.Shared;

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

        public async Task<IActionResult> RestoreToken(string refreshToken)
        {
            var user = await _userRepository.GetByTokenAsync(refreshToken);
            if (user == null)
                return new NotFoundResult();

            var tokenPair = await UpdateToken(user.RoleName, user.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> SignIn(SignInBody body)
        {
            var user = await _userRepository.GetAsync(body.Email);
            if (user == null)
                return new NotFoundResult();

            var inputPasswordHash = Hmac512Provider.Compute(body.Password);
            if (user.PasswordHash != inputPasswordHash)
                return new BadRequestResult();

            var tokenPair = await UpdateToken(user.RoleName, user.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> SignUp(SignUpBody body, string rolename)
        {
            var user = await _userRepository.AddAsync(body, rolename);
            if (user == null)
                return new ConflictResult();

            var tokenPair = await UpdateToken(rolename, user.Id);
            return new OkObjectResult(tokenPair);
        }

        private async Task<TokenPair> UpdateToken(string rolename, Guid userId)
        {
            var tokenInfo = new TokenInfo
            {
                Role = rolename,
                UserId = userId
            };

            var tokenPair = _jwtService.GenerateDefaultTokenPair(tokenInfo);
            tokenPair.RefreshToken = await _userRepository.UpdateTokenAsync(tokenPair.RefreshToken, tokenInfo.UserId);
            return tokenPair;
        }
    }
}
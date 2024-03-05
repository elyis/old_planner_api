using System.Net;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;
using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public ProfileController(
            IUserRepository userRepository,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }


        [HttpGet("profile"), Authorize]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileBody))]
        public async Task<IActionResult> GetProfileAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            return user == null ? NotFound() : Ok(user.ToProfileBody());
        }

        [HttpPatch("userTag"), Authorize]
        [SwaggerOperation("Изменить пользовательский тег")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> ChangeUserTag(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] string userTag
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.UpdateUserTagAsync(tokenInfo.UserId, userTag);
            return user == null ? BadRequest() : Ok();
        }

        [HttpGet("userTag")]
        [SwaggerOperation("Получить профиль пользователя по пользовательскому тегу")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserProfileByUserTag([FromQuery, Required] string userTag)
        {
            var user = await _userRepository.GetByUserTagAsync(userTag);
            return user == null ? NotFound() : Ok(user.ToProfileBody());
        }

        [HttpGet("users/userTag")]
        [SwaggerOperation("Получить список пользователей по паттерну userTag")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersByPatternUserTag([FromQuery, Required] string patternUserTag)
        {
            var users = await _userRepository.GetUsersByPatternUserTag(patternUserTag);
            var result = users.Select(e => e.ToProfileBody());
            return Ok(result);
        }


        [HttpGet("user")]
        [SwaggerOperation("Получить профиль пользователя")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserInfo(
            [FromQuery, Required] string identifier
        )
        {
            var user = await _userRepository.GetAsync(identifier);
            return user == null ? NotFound() : Ok(user.ToProfileBody());
        }


        [HttpGet("users/identifier")]
        [SwaggerOperation("Получить список пользователей по паттерну")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersBy(
            [FromQuery, Required] string identifierPattern
        )
        {
            var users = await _userRepository.GetUsersByPatternIdentifier(identifierPattern);
            var result = users.Select(e => e.ToProfileBody());
            return Ok(result);
        }
    }
}
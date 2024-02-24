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

        [HttpGet("user")]
        [SwaggerOperation("Получить профиль пользователя")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserInfo(
            [FromQuery, EmailAddress] string email
        )
        {
            var user = await _userRepository.GetAsync(email);
            return user == null ? NotFound() : Ok(user.ToProfileBody());
        }


        [HttpGet("users")]
        [SwaggerOperation("Получить список пользователей по паттерну почты")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersBy(
            [FromQuery, Required] string emailPattern
        )
        {
            var users = await _userRepository.GetUsersByPatternEmail(emailPattern);
            var result = users.Select(e => e.ToProfileBody());
            return Ok(result);
        }
    }
}
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Shared;
using old_planner_api.src.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [SwaggerOperation("Регистрация")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(TokenPair))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]
        [SwaggerResponse(409, "Почта уже существует")]

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpBody signUpBody)
        {
            string role = Enum.GetName(UserRole.Common)!;
            var result = await _authService.SignUp(signUpBody, role, AuthenticationProviderType.Default);
            return result;
        }



        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(TokenPair))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody signInBody)
        {
            var result = await _authService.SignIn(signInBody, AuthenticationProviderType.Default);
            return result;
        }

        [SwaggerOperation("Восстановление токена")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(TokenPair))]
        [SwaggerResponse(400, "Идентификатор устройства не валиден")]
        [SwaggerResponse(404, "Токен не используется")]

        [HttpPost("token")]
        public async Task<IActionResult> RestoreTokenAsync(
            TokenBody body,
            [FromHeader(Name = "DeviceId")] string deviceId,
            [FromHeader(Name = "DeviceTypeId")] DeviceTypeId deviceTypeId
        )
        {
            var result = await _authService.RestoreToken(body.Value, deviceId, deviceTypeId);
            return result;
        }
    }
}
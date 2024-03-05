using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using webApiTemplate.src.App.Provider;

namespace old_planner_api.src.Web.Controllers.View
{
    [Route("")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;


        public AccountController(
            ILogger<AccountController> logger,
            IAuthService authService
        )
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

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

            var signUpBody = new SignUpBody
            {
                Identifier = email,
                Method = AuthenticationMethod.Email,
                Nickname = name,
                Password = Hmac512Provider.Compute(nameIdentifier)
            };

            var response = await _authService.SignUp(signUpBody, UserRole.Common.ToString());
            if (response is OkObjectResult okObjectResult)
                return response;

            var signInBody = new SignInBody
            {
                Identifier = email,
                Method = AuthenticationMethod.Email,
                Password = Hmac512Provider.Compute(nameIdentifier)
            };
            var signInResponse = await _authService.SignIn(signInBody);
            return signInResponse;
        }
    }
}
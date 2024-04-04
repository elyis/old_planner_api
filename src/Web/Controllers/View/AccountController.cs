using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
        public IActionResult GoogleLogin([FromQuery(Name = "deviceId"), Required] string deviceId)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };


            var deviceTypeId = GetDeviceType(deviceId);
            if (deviceTypeId == null)
                return BadRequest();

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
            _logger.LogCritical(nameIdentifier);

            var session = HttpContext.Session;
            var deviceTypeId = session.GetString("deviceTypeId");
            var deviceId = session.GetString("deviceId");


            var isExist = await _authService.AccountIsExist(email);
            if (isExist)
            {
                var accountCreatedByGoogle = await _authService.AccountAuthorizedByProvider(email, AuthenticationProviderType.Google);
                if (!accountCreatedByGoogle)
                    return Conflict();

                var signInBody = new SignInBody
                {
                    Identifier = email,
                    Method = AuthenticationMethod.Email,
                    Password = Hmac512Provider.Compute(nameIdentifier),
                    DeviceId = deviceId,
                    DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
                };

                var signInResponse = await _authService.SignIn(signInBody, AuthenticationProviderType.Default);
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
            return response;
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
using old_planner_api.src.Domain.Entities.Request;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.App.IService
{
    public interface IAuthService
    {
        Task<IActionResult> SignUp(SignUpBody body, string rolename, AuthenticationProviderType provider);
        Task<IActionResult> SignIn(SignInBody body, AuthenticationProviderType provider);
        Task<IActionResult> RestoreToken(string refreshToken, string deviceId, DeviceTypeId deviceTypeId);
        Task<bool> AccountIsExist(string identifier);
        Task<bool> AccountAuthorizedByProvider(string identifier, AuthenticationProviderType provider);
        Task<IActionResult> CreateMailCredentials(string email, string? access_token, string? refresh_token, EmailProvider provider);
    }
}
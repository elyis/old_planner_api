using old_planner_api.src.Domain.Entities.Request;
using Microsoft.AspNetCore.Mvc;

namespace old_planner_api.src.App.IService
{
    public interface IAuthService
    {
        Task<IActionResult> SignUp(SignUpBody body, string rolename);
        Task<IActionResult> SignIn(SignInBody body);
        Task<IActionResult> RestoreToken(string refreshToken);
    }
}
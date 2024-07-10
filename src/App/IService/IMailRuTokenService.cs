using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.App.IService
{
    public interface IMailRuTokenService
    {
        string GetAuthorizationUrl();
        Task<MailruTokenResponse?> GetTokenAsync(string code);
        Task<AccessTokenWithLifetimeBody?> UpdateToken(string refresh_token);
        Task<MailUserInfoBody?> GetUserInfo(string accessToken);
    }
}
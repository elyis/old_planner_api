using System.Security.Claims;
using old_planner_api.src.Domain.Entities.Shared;
using webApiTemplate.src.Domain.Entities.Shared;

namespace webApiTemplate.src.App.IService
{
    public interface IJwtService
    {
        string GenerateAccessToken(Dictionary<string, string> claims, TimeSpan timeSpan);
        string GenerateRefreshToken() => Guid.NewGuid().ToString();
        TokenPair GenerateDefaultTokenPair(TokenInfo tokenInfo);
        TokenPair GenerateTokenPair(Dictionary<string, string> claims, TimeSpan timeSpan);
        List<Claim> GetClaims(string token);
        TokenInfo GetTokenInfo(string token);
    }
}
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.App.IService
{
    public interface IGoogleTokenService
    {
        Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken);
    }
}
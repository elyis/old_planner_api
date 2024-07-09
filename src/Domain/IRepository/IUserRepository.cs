using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserModel?> AddAsync(SignUpBody body, string role, AuthenticationProviderType providerType);
        Task<UserSession?> GetSessionAsync(Guid userId, string deviceId);
        Task<UserSession?> GetSessionAsync(Guid sessionId);
        Task<UserSession?> AddUserSessionAsync(string deviceId, UserModel user);
        Task<List<UserSession>> GetUserSessionsAsync(Guid userId);
        Task<UserModel?> GetAsync(Guid id);
        Task<UserModel?> GetByUserTagAsync(string userTag);
        Task<List<UserModel>> GetUsersByPatternUserTag(string patternUserTag);
        Task<List<UserModel>> GetUsersAsync(List<string> identifiers);
        Task<List<UserModel>> GetUsersByPatternIdentifier(string identifier);
        Task<UserModel?> UpdateUserTagAsync(Guid id, string userTag);
        Task<UserModel?> GetAsync(string identifier);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<UserSession?> GetUserSessionByTokenAndUser(string refreshTokenHash);
        Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename);
        Task<List<UserMailCredentials>> GetUserMailCredentials(Guid userId);
        Task<UserMailCredentials?> AddUserMailCredential(string email, string access_token, string refresh_token, UserModel user, EmailProvider emailProvider);
        Task<UserMailCredentials?> GetUserMailCredential(string email);
    }
}
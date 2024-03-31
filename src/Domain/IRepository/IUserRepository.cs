using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserModel?> AddAsync(SignUpBody body, string role);
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
        Task<string?> UpdateTokenAsync(string refreshToken, Guid userId, TimeSpan? duration = null);
        Task<UserModel?> GetByTokenAsync(string refreshTokenHash);
        Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename);
    }
}
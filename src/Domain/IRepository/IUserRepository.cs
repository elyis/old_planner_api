using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserModel?> AddAsync(SignUpBody body, string role);
        Task<UserModel?> GetAsync(Guid id);
        Task<UserModel?> GetAsync(string email);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid userId, TimeSpan? duration = null);
        Task<UserModel?> GetByTokenAsync(string refreshTokenHash);
        Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename);
    }
}
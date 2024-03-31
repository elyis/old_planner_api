using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using webApiTemplate.src.App.Provider;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> AddAsync(SignUpBody body, string role)
        {
            var oldUser = await GetAsync(body.Identifier);
            if (oldUser != null)
                return null;

            var newUser = new UserModel
            {
                Identifier = body.Identifier,
                AuthenticationMethod = body.Method.ToString(),
                Nickname = body.Nickname,
                PasswordHash = Hmac512Provider.Compute(body.Password),
                RoleName = role,
            };

            var result = await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<UserSession?> GetSessionAsync(Guid userId, string deviceId)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(e => e.UserId == userId && e.DeviceId == deviceId);
        }

        public async Task<List<UserSession>> GetUserSessionsAsync(Guid userId)
        {
            return await _context.UserSessions
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserSession?> AddUserSessionAsync(string deviceId, UserModel user)
        {
            var userSession = await GetSessionAsync(user.Id, deviceId);
            if (userSession != null)
                return null;

            userSession = new UserSession
            {
                DeviceId = deviceId,
                User = user,
            };

            var result = await _context.UserSessions.AddAsync(userSession);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }


        public async Task<List<UserModel>> GetUsersAsync(List<string> identifiers)
        {
            var result = await _context.Users.Where(e => identifiers.Contains(e.Identifier)).ToListAsync();
            return result;
        }

        public async Task<UserModel?> GetAsync(Guid id)
            => await _context.Users
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<UserModel?> GetAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(e => e.Identifier == identifier);
        }

        public async Task<UserModel?> GetByTokenAsync(string refreshTokenHash)
            => await _context.Users
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<List<UserModel>> GetUsersByPatternIdentifier(string identifier)
        {
            return await _context.Users
                .Where(e => EF.Functions.Like(e.Identifier, $"%{identifier}%"))
                .ToListAsync();
        }


        public async Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename)
        {
            var user = await GetAsync(userId);
            if (user == null)
                return null;

            user.Image = filename;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid userId, TimeSpan? duration = null)
        {
            var user = await GetAsync(userId);
            if (user == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(15);

            if (user.TokenValidBefore <= DateTime.UtcNow || user.TokenValidBefore == null)
            {
                user.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                user.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            return user.Token;
        }

        public async Task<UserModel?> UpdateUserTagAsync(Guid id, string userTag)
        {
            var user = await GetAsync(id);
            if (user == null)
                return null;

            var existedUser = await GetByUserTagAsync(userTag);
            if (existedUser != null)
                return null;

            user.UserTag = userTag;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserModel?> GetByUserTagAsync(string userTag)
            => await _context.Users.FirstOrDefaultAsync(e => e.UserTag == userTag);

        public async Task<List<UserModel>> GetUsersByPatternUserTag(string patternUserTag)
        {
            return await _context.Users
               .Where(e => e.UserTag != null && EF.Functions.Like(e.UserTag, $"%{patternUserTag}%"))
               .ToListAsync();
        }

        public async Task<UserSession?> GetSessionAsync(Guid sessionId)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
        }
    }
}
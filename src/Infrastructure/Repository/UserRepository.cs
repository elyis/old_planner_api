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
            var oldUser = await GetAsync(body.Email);
            if (oldUser != null)
                return null;

            var newUser = new UserModel
            {
                Email = body.Email,
                PasswordHash = Hmac512Provider.Compute(body.Password),
                RoleName = role,
            };

            var result = await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }


        public async Task<List<UserModel>> GetUsersAsync(List<string> userEmails)
        {
            var result = await _context.Users.Where(e => userEmails.Contains(e.Email)).ToListAsync();
            return result;
        }

        public async Task<UserModel?> GetAsync(Guid id)
            => await _context.Users
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<UserModel?> GetAsync(string email)
            => await _context.Users
                .FirstOrDefaultAsync(e => e.Email == email);

        public async Task<UserModel?> GetByTokenAsync(string refreshTokenHash)
            => await _context.Users
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<List<UserModel>> GetUsersByPatternEmail(string email)
        {
            return await _context.Users
                .Where(e => EF.Functions.Like(e.Email, $"%{email}%"))
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
    }
}
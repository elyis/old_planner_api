using old_planner_api.src.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace old_planner_api.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public AppDbContext
        (
            DbContextOptions<AppDbContext> options,
            IConfiguration config
        ) : base(options)
        {
            _config = config;
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<DeletedTask> DeletedTasks { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<TaskChat> Chats { get; set; }
        public DbSet<UserChatHistory> UserChatHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("Default");
            optionsBuilder.UseSqlite(connectionString);
            // optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardMember>().HasKey(e => new
            {
                e.BoardId,
                e.UserId
            });

            modelBuilder.Entity<UserChatHistory>().HasKey(e => new
            {
                e.ParticipantId,
                e.ChatId
            });

            base.OnModelCreating(modelBuilder);
        }
    }

}
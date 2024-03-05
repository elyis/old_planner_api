using old_planner_api.src.Domain.Models;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<TaskChatMessage> TaskChatMessages { get; set; }
        public DbSet<TaskChat> TaskChats { get; set; }
        public DbSet<TaskChatMembership> TaskChatMemberships { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMembership> ChatMemberships { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("Default");
            optionsBuilder.UseNpgsql(connectionString);
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

            modelBuilder.Entity<TaskChatMembership>().HasKey(e => new
            {
                e.ParticipantId,
                e.ChatId
            });

            modelBuilder.Entity<ChatMembership>().HasKey(e => new
            {
                e.UserId,
                e.ChatId
            });

            base.OnModelCreating(modelBuilder);
        }
    }

}
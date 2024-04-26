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
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<TaskChatMessage> TaskChatMessages { get; set; }
        public DbSet<TaskChat> TaskChats { get; set; }
        public DbSet<TaskChatMembership> TaskChatMemberships { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMembership> ChatMemberships { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserChatSession> UserChatSessions { get; set; }
        public DbSet<UserTaskChatSession> UserTaskChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
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

            base.OnModelCreating(modelBuilder);
        }
    }

}
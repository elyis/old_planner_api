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
        public DbSet<BoardColumnMember> ColumnMembers { get; set; }
        public DbSet<UserMailCredentials> UserMailCredentials { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMembership> ChatMemberships { get; set; }
        public DbSet<BoardColumnTask> BoardColumnTasks { get; set; }
        public DbSet<TaskPerformer> TaskPerformers { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserChatSession> UserChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<TaskAttachedMessage> TaskAttachedMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardMember>().HasKey(e => new
            {
                e.BoardId,
                e.UserId
            });

            modelBuilder.Entity<BoardColumnMember>().HasKey(e => new
            {
                e.UserId,
                e.ColumnId
            });

            modelBuilder.Entity<BoardColumnTask>().HasKey(e => new
            {
                e.TaskId,
                e.ColumnId
            });

            modelBuilder.Entity<TaskPerformer>().HasKey(e => new
            {
                e.PerformerId,
                e.TaskId
            });

            modelBuilder.Entity<TaskAttachedMessage>().HasKey(e => new
            {
                e.TaskId,
                e.MessageId
            });

            base.OnModelCreating(modelBuilder);
        }
    }

}
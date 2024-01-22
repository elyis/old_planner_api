using old_planner_api.src.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace old_planner_api.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<TaskDraft> Drafts { get; set; }
        public DbSet<DeletedTask> DeletedTasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("Default");
            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

}
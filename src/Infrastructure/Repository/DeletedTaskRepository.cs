using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;


namespace old_planner_api.src.Infrastructure.Repository
{
    public class DeletedTaskRepository : IDeletedTaskRepository
    {
        private readonly AppDbContext _context;

        public DeletedTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DeletedTask?> AddAsync(TaskModel deletedTask)
        {
            var task = await GetByTaskId(deletedTask.Id);
            if(task != null)
                return null;
            
            task = new DeletedTask
            {
                Task = deletedTask,
                ExistBeforeDate = DateTime.UtcNow.AddDays(7),
            };

            task = (await _context.DeletedTasks.AddAsync(task))?.Entity;
            deletedTask.Status = TaskState.Deleted.ToString();
            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<IEnumerable<DeletedTask>> GetAll()
            => await _context.DeletedTasks
                .Include(e => e.Task)
                .ToListAsync();

        public async Task<DeletedTask?> GetByTaskId(Guid taskId)
            => await _context.DeletedTasks
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

        public async Task<bool> RemoveAsync(Guid id)
        {
            var deletedTask = await _context.DeletedTasks
                .Include(e => e.Task)
                .FirstOrDefaultAsync(e => e.Id == id);

            if(deletedTask == null)
                return true;

            _context.DeletedTasks.Remove(deletedTask);
            deletedTask.Task.Status = TaskState.Undefined.ToString();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskModel?> AddAsync(CreateTaskBody taskBody)
        {
            var addedTask = new TaskModel
            {
                Title = taskBody.Title,
                Description = taskBody.Description,
                HexColor = taskBody.HexColor,
                PriorityOrder = taskBody.PriorityOrder,
                Status = taskBody.Status.ToString(),
                StartDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate),
                EndDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate),
            };

            addedTask = (await _context.Tasks.AddAsync(addedTask))?.Entity;
            await _context.SaveChangesAsync();
            return addedTask;
        }

        public async Task<IEnumerable<TaskModel>> GetAll()
            => await _context.Tasks
                .ToListAsync();

        public async Task<IEnumerable<TaskModel>> GetAllByStatus(TaskState status)
        {
            var statusString = status.ToString();
            var tasks = await _context.Tasks
                .Where(e => e.Status == statusString)
                .ToListAsync();

            return tasks;
        }

        public async Task<TaskModel?> GetAsync(Guid id)
            => await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<bool> RemoveAsync(Guid id)
        {
            var task = await GetAsync(id);
            if(task == null)
                return true;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskModel?> UpdateAsync(UpdateTaskBody taskBody)
        {
            var task = await GetAsync(taskBody.Id);
            if(task == null)
                return null;

            task.Title = taskBody.Title;
            task.Description = taskBody.Description;
            task.HexColor = taskBody.HexColor;
            task.PriorityOrder = taskBody.PriorityOrder;
            task.Status = taskBody.Status.ToString();
            task.StartDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
            task.EndDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);
            
            await _context.SaveChangesAsync();
            return task;
        }
    }
}
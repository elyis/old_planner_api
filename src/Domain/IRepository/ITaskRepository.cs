using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(CreateTaskBody taskBody);
        Task<TaskModel?> GetAsync(Guid id);
        Task<bool> RemoveAsync(Guid id);
        Task<IEnumerable<TaskModel>> GetAllByStatus(TaskState status);
        Task<IEnumerable<TaskModel>> GetAll();
        Task<TaskModel?> UpdateAsync(UpdateTaskBody taskBody);
    }
}
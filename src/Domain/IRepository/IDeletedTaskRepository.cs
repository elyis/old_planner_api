using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IDeletedTaskRepository
    {
        Task<DeletedTask?> AddAsync(TaskModel deletedTask);
        Task<DeletedTask?> GetByTaskId(Guid taskId);
        Task<bool> RemoveAsync(Guid id);
        Task<IEnumerable<DeletedTask>> GetAll();
    }
}
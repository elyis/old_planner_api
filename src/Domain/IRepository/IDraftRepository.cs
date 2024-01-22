using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IDraftRepository
    {
        Task<TaskDraft?> AddAsync(CreateDraftBody draftBody, TaskModel? modifiedTask);
        Task<TaskDraft?> GetAsync(Guid id);
        Task<bool> RemoveAsync(Guid id);
        Task<TaskDraft?> UpdateAsync(UpdateDraftBody draftBody);
        Task<IEnumerable<TaskDraft>> GetAllByTaskId(Guid taskId);
    }
}
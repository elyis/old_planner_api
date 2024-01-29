using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(CreateTaskBody taskBody, Board board, UserModel creator);
        Task<TaskModel?> AddAsync(CreateDraftBody draftBody, Board board, UserModel creator, TaskModel? parentTask);
        Task<TaskModel?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        Task<IEnumerable<TaskModel>> GetAll(Guid boardId, TaskState? status, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid boardId, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAllDrafts(Guid boardId, Guid userId);
        Task<IEnumerable<TaskModel>> GetAll(Guid boardId);
        Task<TaskModel?> ConvertDraftToTask(Guid id, UserModel user);
        Task<TaskModel?> UpdateAsync(UpdateTaskBody taskBody);
        Task<TaskModel?> UpdateAsync(UpdateDraftBody draftBody, TaskModel? draftOfTask);
    }
}
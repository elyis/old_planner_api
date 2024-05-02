using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(CreateTaskBody taskBody, BoardColumn column, UserModel creator);
        Task<TaskModel?> AddAsync(CreateDraftBody draftBody, BoardColumn column, UserModel creator, TaskModel? parentTask);
        Task<TaskModel?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, TaskState? status, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId);
        Task<TaskModel?> ConvertDraftToTask(Guid id, UserModel user, BoardColumn column);
        Task<TaskModel?> UpdateAsync(UpdateTaskBody taskBody);
        Task<TaskModel?> UpdateAsync(UpdateDraftBody draftBody, TaskModel? draftOfTask);
        Task<TaskPerformer?> AddTaskPerformer(TaskModel task, UserModel performer);
        Task<IEnumerable<TaskPerformer>> AddTaskPerformers(TaskModel task, IEnumerable<UserModel> performers);
        Task<TaskPerformer?> GetTaskPerformer(Guid performerId, Guid taskId);
        Task<IEnumerable<TaskPerformer>> GetTaskPerformers(IEnumerable<Guid> performerIds, Guid taskId, int count, int offset);
        Task<IEnumerable<TaskPerformer>> GetTaskPerformers(Guid taskId, int count, int offset);
        Task AddTaskToColumn(TaskModel tasks, BoardColumn column);
    }
}
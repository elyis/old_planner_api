using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskChatRepository
    {
        Task<TaskChat?> AddAsync(TaskModel task);
        Task<TaskChatMessage?> AddAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender);
        Task<TaskChat?> GetByTaskAsync(Guid taskId);
        Task<TaskChat?> GetAsync(Guid id);
        Task<TaskChatMessage?> GetMessageAsync(Guid id);
        Task<List<TaskChatMembership>> GetTaskChatMemberships(Guid userId);
        Task<TaskChat?> GetTaskChatWithParticipants(Guid taskId);
        Task<IEnumerable<TaskChatMessage>> GetLastMessages(TaskChatMembership chatHistory, DateTime startedTime, int count);
        Task<TaskChatMembership?> GetUserChatHistoryAsync(Guid chatId, Guid userId);
        Task<TaskChatMembership?> CreateOrGetUserChatHistoryAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> AddAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId);
    }
}
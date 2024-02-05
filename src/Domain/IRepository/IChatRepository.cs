using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IChatRepository
    {
        Task<TaskChat?> AddAsync(TaskModel task);
        Task<ChatMessage?> AddAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender);
        Task<TaskChat?> GetByTaskAsync(Guid taskId);
        Task<TaskChat?> GetAsync(Guid id);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<IEnumerable<ChatMessage>> GetLastMessages(UserChatHistory chatHistory, DateTime startedTime, int count);
        Task<UserChatHistory?> GetUserChatHistoryAsync(Guid chatId, Guid userId);
        Task<UserChatHistory?> CreateOrGetUserChatHistoryAsync(TaskChat chat, UserModel user);
    }
}
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskChatRepository
    {
        Task<TaskChatMessage?> AddMessageAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender);
        Task<TaskChat?> GetChatAsync(Guid id);
        Task<TaskChatMessage?> GetMessageAsync(Guid id);
        Task<List<TaskChatBody>> GetUserChatBodies(Guid userId);
        Task<IEnumerable<TaskChatMessage>> GetLastMessages(TaskChatMembership chatHistory, DateTime startedTime, int count);
        Task<TaskChatMembership?> AddOrGetUserChatHistoryAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> AddChatMembershipAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId);
        Task<bool> UpdateLastViewingChatMembership(TaskChatMembership chatMembership, DateTime lastViewingDate);
    }
}
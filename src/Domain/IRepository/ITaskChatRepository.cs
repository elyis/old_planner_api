using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface ITaskChatRepository
    {
        Task<TaskChatMessage?> AddMessageAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender);
        Task<TaskChat?> GetChatAsync(Guid id);
        Task<TaskChat?> UpdateChatImage(Guid chatId, string filename);
        Task<TaskChatMessage?> GetMessageAsync(Guid id);
        Task<List<TaskChatBody>> GetUserChatBodies(Guid userId);
        Task<List<TaskChatMembership>> GetChatMembershipsAsync(Guid chatId);
        Task<List<TaskChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true);
        Task<TaskChatMembership?> AddOrGetChatMembershipAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> AddChatMembershipAsync(TaskChat chat, UserModel user);
        Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId);
        Task<bool> UpdateLastViewingChatMembership(TaskChatMembership chatMembership, DateTime lastViewingDate);
    }
}
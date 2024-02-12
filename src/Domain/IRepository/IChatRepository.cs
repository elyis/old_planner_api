using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetAsync(Guid id);
        Task<ChatMembership?> AddMembershipAsync(UserModel user, Chat chat);
        Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid userId);
        Task<ChatMessage?> AddAsync(CreateMessageBody messageBody, Chat chat, UserModel sender);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<IEnumerable<ChatMessage>> GetLastMessages(ChatMembership chatMembership, DateTime startedTime, int count);
        Task<ChatMembership?> CreateOrGetChatMembershipAsync(Chat chat, UserModel user);
        Task<ChatMembership?> AddAsync(Chat chat, UserModel user);
        Task<List<ChatMembership>> GetUserChatMemberships(Guid userId);
        Task<ChatMembership?> GetChatMembershipAsync(Guid firstUserId, Guid secondUserId, ChatType? chatType);
        Task<int> GetCountChatMemberships(Guid chatId);
        Task<Chat?> AddChatAsync(List<UserModel> participants, CreateChatBody chatBody);
    }
}
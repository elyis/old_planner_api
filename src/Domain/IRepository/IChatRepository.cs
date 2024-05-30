using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetAsync(Guid id);
        Task<ChatMembership?> AddMembershipAsync(UserModel user, Chat chat);
        Task<List<ChatMembership>> GetChatMembershipsAsync(Guid chatId);
        Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid userId);
        Task<List<ChatMessage>> GetMessages(IEnumerable<Guid> messageIds);
        Task<ChatMessage?> AddMessageAsync(CreateMessageBody messageBody, Chat chat, UserModel sender);
        Task CreateUserChatSessionAsync(IEnumerable<UserSession> sessions, ChatMembership chatMembership, DateTime date);
        Task CreateUserChatSessionsAsync(UserSession session);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<Chat?> UpdateChatImage(Guid chatId, string filename);
        Task<UserChatSession?> GetUserChatSessionAsync(Guid sessionId, Guid chatMembershipId);
        Task<bool> UpdateLastViewingUserChatSession(UserChatSession userChatSession, DateTime lastViewingDate);
        Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true);
        Task<bool> UpdateLastViewingChatMembership(ChatMembership chatMembership, DateTime lastViewingDate);
        Task<ChatMembership?> CreateOrGetChatMembershipAsync(Chat chat, UserModel user);
        Task<List<ChatBody>> GetChatBodies(Guid userId, Guid userSessionId, ChatType chatType);
        Task<ChatMembership?> GetPersonalChatAsync(Guid firstUserId, Guid secondUserId);
        Task<Chat?> AddPersonalChatAsync(List<UserModel> participants, CreateChatBody chatBody, DateTime date);
        Task<Chat?> GetByTaskIdAsync(Guid taskId);
    }
}
using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Chat?> AddPersonalChatAsync(List<UserModel> participants, CreateChatBody chatBody, DateTime date)
        {
            if (participants.Count != 2 || (await GetPersonalChatAsync(participants[0].Id, participants[1].Id) != null))
                return null;

            var memberships = participants
                .Select(user => new ChatMembership
                {
                    User = user,
                    DateLastViewing = date
                })
                .ToList();

            var chat = new Chat
            {
                Name = chatBody.Name,
                Type = ChatType.Personal.ToString(),
                ChatMemberships = memberships,
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task CreateUserChatSessionAsync(IEnumerable<UserSession> sessions, ChatMembership chatMembership, DateTime date)
        {
            var userChatSessions = sessions
                .Select(e => new UserChatSession
                {
                    Session = e,
                    ChatMembership = chatMembership,
                    DateLastViewing = date
                })
            .ToList();

            await _context.UserChatSessions.AddRangeAsync(userChatSessions);
            await _context.SaveChangesAsync();
        }

        public async Task<Chat?> UpdateChatImage(Guid chatId, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            var chat = await GetAsync(chatId);
            if (chat == null)
                return null;

            chat.Image = filename;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<ChatMembership?> AddMembershipAsync(UserModel user, Chat chat)
        {
            var chatMembership = await GetMembershipAsync(chat.Id, user.Id);
            if (chatMembership != null)
                return null;

            chatMembership = new ChatMembership
            {
                User = user,
                Chat = chat,
            };
            chatMembership = (await _context.ChatMemberships.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();

            return chatMembership;
        }

        public async Task<List<ChatMembership>> GetChatMembershipsAsync(Guid chatId)
        {
            return await _context.ChatMemberships
                .Where(e => e.ChatId == chatId)
                .ToListAsync();
        }

        public async Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid userId)
            => await _context.ChatMemberships
                .FirstOrDefaultAsync(e => e.ChatId == chatId && e.UserId == userId);

        public async Task<Chat?> GetAsync(Guid id)
            => await _context.Chats.FindAsync(id);

        public async Task<ChatMembership?> GetPersonalChatAsync(Guid firstUserId, Guid secondUserId)
        {
            var query = _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(firstUser =>
                    firstUser.UserId == firstUserId &&
                    _context.ChatMemberships.Any(secondUser =>
                        secondUser.UserId == secondUserId &&
                        secondUser.ChatId == firstUser.ChatId)
                );

            var result = await query.FirstOrDefaultAsync(e => e.Chat.Type == ChatType.Personal.ToString());
            return result;
        }

        public async Task<ChatMessage?> AddMessageAsync(CreateMessageBody messageBody, Chat chat, UserModel sender)
        {
            var message = new ChatMessage
            {
                Chat = chat,
                Type = messageBody.Type.ToString(),
                Content = messageBody.Content,
                Sender = sender,
            };

            message = (await _context.ChatMessages.AddAsync(message))?.Entity;
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatMessage?> GetMessageAsync(Guid id)
            => await _context.ChatMessages.FindAsync(id);

        public async Task<bool> UpdateLastViewingChatMembership(ChatMembership chatMembership, DateTime lastViewingDate)
        {
            if (chatMembership == null)
                return false;

            if (chatMembership.DateLastViewing > lastViewingDate)
                return false;

            chatMembership.DateLastViewing = lastViewingDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatMembership?> CreateOrGetChatMembershipAsync(Chat chat, UserModel user)
        {
            var chatMembership = await GetMembershipAsync(chat.Id, user.Id);
            if (chatMembership != null)
                return chatMembership;

            chatMembership = new ChatMembership
            {
                Chat = chat,
                User = user
            };

            chatMembership = (await _context.ChatMemberships.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();
            return chatMembership;
        }

        public async Task<ChatMembership?> AddUserToChatAsync(Chat chat, UserModel user)
        {
            if (chat.Type == ChatType.Personal.ToString())
            {
                var memberships = await _context.ChatMemberships.CountAsync(e => e.ChatId == chat.Id);
                if (memberships >= 2)
                    return null;
            }

            var membership = await GetMembershipAsync(chat.Id, user.Id);
            if (membership != null)
                return null;

            membership = new ChatMembership
            {
                Chat = chat,
                User = user
            };

            membership = (await _context.ChatMemberships.AddAsync(membership))?.Entity;
            await _context.SaveChangesAsync();

            return membership;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true)
        {
            var query = _context.ChatMessages
                .Where(e => e.ChatId == chatId);

            query = isDescending
                ? query.OrderByDescending(e => e.SentAt)
                : query.OrderBy(e => e.SentAt);

            return await query
                .Skip(countSkipped)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ChatBody>> GetUserChats(Guid userId, Guid userSessionId)
        {
            var result = new List<ChatBody>();

            var userChatMemberships = await _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            if (!userChatMemberships.Any())
                return result;

            var chatIds = userChatMemberships.Select(e => e.ChatId);

            var chatMemberships = await _context.ChatMemberships
                .Include(e => e.User)
                .Where(e => chatIds.Contains(e.ChatId))
                .GroupBy(e => e.ChatId)
                .ToListAsync();

            foreach (var chatMembership in chatMemberships)
            {
                var chatId = chatMembership.Key;
                var userMembership = userChatMemberships.First(e => e.ChatId == chatId);
                var userSession = await GetUserChatSessionAsync(userSessionId, userMembership.Id);

                var chat = userMembership.Chat;
                var dateLastViewing = userSession.DateLastViewing;

                var countOfUnreadMessages = await _context.ChatMessages
                    .CountAsync(e => e.SentAt > dateLastViewing && e.ChatId == chatId);

                var lastMessage = await _context.ChatMessages
                        .Where(e => e.SentAt > dateLastViewing && e.ChatId == chatId)
                        .OrderByDescending(e => e.SentAt)
                        .FirstOrDefaultAsync();

                var chatBody = new ChatBody
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    ImageUrl = chat.Image == null ? null : $"{Constants.webPathToPrivateChatIcons}{chat.Image}",
                    CountOfUnreadMessages = countOfUnreadMessages,
                    IsSyncedReadStatus = userSession.DateLastViewing == userMembership.DateLastViewing,
                    Participants = chatMembership.Select(e => e.User.ToChatUserInfo()).ToList(),
                    LastMessage = lastMessage?.ToMessageBody()
                };
                result.Add(chatBody);
            }

            return result;
        }

        public async Task<UserChatSession?> GetUserChatSessionAsync(Guid sessionId, Guid chatMembershipId)
        {
            return await _context.UserChatSessions
                .FirstOrDefaultAsync(e => e.SessionId == sessionId && e.ChatMembershipId == chatMembershipId);
        }

        public async Task<int> GetCountChatMemberships(Guid chatId)
            => await _context.ChatMemberships.CountAsync(e => e.ChatId == chatId);

        public async Task<bool> UpdateLastViewingUserChatSession(UserChatSession userChatSession, DateTime lastViewingDate)
        {
            if (userChatSession == null)
                return false;

            if (userChatSession.DateLastViewing > lastViewingDate)
                return false;

            userChatSession.DateLastViewing = lastViewingDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateUserChatSessionAsync(UserSession session)
        {
            var chatMemberships = await _context.ChatMemberships.Where(e => e.UserId == session.UserId).ToListAsync();

            var userChatSessions = chatMemberships
                .Select(e => new UserChatSession
                {
                    Session = session,
                    ChatMembership = e,
                    DateLastViewing = e.DateLastViewing
                });

            await _context.UserChatSessions.AddRangeAsync(userChatSessions);
            await _context.SaveChangesAsync();
        }
    }
}
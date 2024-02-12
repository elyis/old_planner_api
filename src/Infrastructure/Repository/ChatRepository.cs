using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
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

        public async Task<Chat?> AddChatAsync(List<UserModel> participants, CreateChatBody chatBody)
        {
            if (participants.Count < 2 || (chatBody.Type == ChatType.Personal && participants.Count != 2))
                return null;

            if (chatBody.Type == ChatType.Personal && (await GetChatMembershipAsync(participants[0].Id, participants[1].Id, ChatType.Personal) != null))
                return null;

            var memberships = participants
            .Select(user => new ChatMembership
            {
                User = user
            })
            .ToList();

            var chat = new Chat
            {
                Name = chatBody.Name,
                Type = chatBody.Type.ToString(),
                ChatMemberships = memberships,
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<ChatMessage?> AddMessage(CreateMessageBody messageBody, Chat chat, UserModel sender)
        {
            var message = new ChatMessage
            {
                Chat = chat,
                Sender = sender,
                Type = messageBody.Type.ToString(),
                Content = messageBody.Content,
            };

            message = (await _context.ChatMessages.AddAsync(message))?.Entity;
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<List<ChatMessage>> GetLastMessages(ChatMembership chatMembership, DateTime startedTime, int count, bool isUpdateViewing = false)
        {
            var messages = await _context.ChatMessages
                .OrderBy(e => e.SentAt)
                .Where(e => e.SentAt > startedTime && e.ChatId == chatMembership.ChatId)
                .Take(count)
                .ToListAsync();

            var lastMessage = messages.LastOrDefault();
            if (lastMessage != null)
            {
                chatMembership.DateLastViewing = lastMessage.SentAt;
                await _context.SaveChangesAsync();
            }

            return messages;
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

        public async Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid userId)
            => await _context.ChatMemberships
                .FirstOrDefaultAsync(e => e.ChatId == chatId && e.UserId == userId);

        public async Task<Chat?> GetAsync(Guid id)
            => await _context.Chats.FindAsync(id);

        public async Task<ChatMembership?> GetChatMembershipAsync(Guid firstUserId, Guid secondUserId, ChatType? chatType)
        {
            var query = _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(firstUser =>
                    firstUser.UserId == firstUserId &&
                    _context.ChatMemberships.Any(secondUser =>
                        secondUser.UserId == secondUserId &&
                        secondUser.ChatId == firstUser.ChatId)
                );

            if (chatType != null)
                query = query.Where(e => e.Chat.Type == chatType.ToString());

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ChatMessage?> AddAsync(CreateMessageBody messageBody, Chat chat, UserModel sender)
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

        public async Task<IEnumerable<ChatMessage>> GetLastMessages(ChatMembership chatMembership, DateTime startedTime, int count)
        {
            var messages = await _context.ChatMessages
                .OrderBy(e => e.SentAt)
                .Where(e => e.SentAt > startedTime && e.ChatId == chatMembership.ChatId)
                .Take(count)
                .ToListAsync();

            var lastMessage = messages.LastOrDefault();
            if (lastMessage != null)
            {
                chatMembership.DateLastViewing = lastMessage.SentAt;
                await _context.SaveChangesAsync();
            }

            return messages;
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

        public async Task<ChatMembership?> AddAsync(Chat chat, UserModel user)
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

        public async Task<List<ChatMembership>> GetUserChatMemberships(Guid userId)
        {
            var chatIds = await _context.ChatMemberships
                .Where(e => e.UserId == userId)
                .Select(e => e.ChatId)
                .ToListAsync();

            var chatMemberships = await _context.ChatMemberships
                .Include(e => e.User)
                .Where(e => chatIds.Contains(e.ChatId))
                .ToListAsync();

            return chatMemberships;
        }

        public async Task<int> GetCountChatMemberships(Guid chatId)
            => await _context.ChatMemberships.CountAsync(e => e.ChatId == chatId);
    }
}
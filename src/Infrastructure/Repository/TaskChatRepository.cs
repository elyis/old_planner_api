using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class TaskChatRepository : ITaskChatRepository
    {
        private readonly AppDbContext _context;

        public TaskChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskChatBody>> GetUserChatBodies(Guid userId)
        {
            var result = new List<TaskChatBody>();

            var chats = await _context.TaskChatMemberships
                .Include(e => e.Chat)
                .Where(e => e.ParticipantId == userId)
                .ToListAsync();

            if (!chats.Any())
                return result;

            var chatIds = chats.Select(e => e.ChatId);

            var chatMemberships = await _context.TaskChatMemberships
                .Include(e => e.Participant)
                .Where(e => chatIds.Contains(e.ChatId))
                .GroupBy(e => e.ChatId)
                .ToListAsync();



            foreach (var chatMembership in chatMemberships)
            {
                var userMembership = chats.First(e => e.ChatId == chatMembership.Key);
                var chat = userMembership.Chat;

                var lastMessage = await _context.TaskChatMessages
                    .FirstOrDefaultAsync(e => e.SentAt > userMembership.DateLastViewing);

                var chatBody = new TaskChatBody
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    ImageUrl = chat.Image == null ? null : $"{Constants.webPathToTaskChatIcons}{chat.Image}",
                    Participants = chatMembership.Select(e => e.Participant.ToChatUserInfo()).ToList(),
                    LastMesssage = lastMessage?.ToMessageBody()
                };
                result.Add(chatBody);
            }

            return result;
        }

        public async Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId)
            => await _context.TaskChatMemberships
                .FirstOrDefaultAsync(e =>
                    e.ChatId == chatId && e.ParticipantId == userId);

        public async Task<List<TaskChatMembership>> GetChatMembershipsAsync(Guid chatId)
            => await _context.TaskChatMemberships
                .Where(e => e.ChatId == chatId)
                .ToListAsync();

        public async Task<TaskChat?> UpdateChatImage(Guid chatId, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            var chat = await GetChatAsync(chatId);
            if (chat == null)
                return null;

            chat.Image = filename;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<TaskChatMembership?> AddOrGetChatMembershipAsync(TaskChat chat, UserModel user)
        {
            var chatMembership = await GetTaskChatMembershipAsync(chat.Id, user.Id);
            if (chatMembership != null)
                return chatMembership;

            chatMembership = new TaskChatMembership
            {
                Chat = chat,
                Participant = user
            };

            chatMembership = (await _context.TaskChatMemberships.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();
            return chatMembership;
        }

        public async Task<TaskChatMessage?> AddMessageAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender)
        {
            var message = new TaskChatMessage
            {
                Chat = chat,
                Type = messageBody.Type.ToString(),
                Content = messageBody.Content,
                Sender = sender,
            };

            message = (await _context.TaskChatMessages.AddAsync(message))?.Entity;
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<TaskChatMessage?> GetMessageAsync(Guid id)
            => await _context.TaskChatMessages.FindAsync(id);

        public async Task<TaskChat?> GetChatAsync(Guid id)
            => await _context.TaskChats.FindAsync(id);

        public async Task<IEnumerable<TaskChatMessage>> GetLastMessages(TaskChatMembership chatMembership, DateTime startedTime, int count)
        {
            var messages = await _context.TaskChatMessages
                .OrderBy(e => e.SentAt)
                .Where(e => e.SentAt >= startedTime && e.ChatId == chatMembership.ChatId)
                .Take(count)
                .ToListAsync();

            return messages;
        }

        public async Task<List<TaskChatMessage>> GetChatMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true)
        {
            var query = _context.TaskChatMessages
                .Where(e => e.ChatId == chatId);

            query = isDescending
                ? query.OrderByDescending(e => e.SentAt)
                : query.OrderBy(e => e.SentAt);

            return await query
                .Skip(countSkipped)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskChatMessage>> GetLastMessagesAndUpdateLastViewing(TaskChatMembership chatMembership, DateTime startedTime, int count)
        {
            var messages = await GetLastMessages(chatMembership, startedTime, count);
            if (messages.Any())
            {
                var lastMessage = messages.Last();
                await UpdateLastViewingChatMembership(chatMembership, lastMessage.SentAt);
            }

            return messages;
        }

        public async Task<bool> UpdateLastViewingChatMembership(TaskChatMembership chatMembership, DateTime lastViewingDate)
        {
            if (chatMembership == null)
                return false;

            if (chatMembership.DateLastViewing > lastViewingDate)
                return false;

            chatMembership.DateLastViewing = lastViewingDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskChatMembership?> AddChatMembershipAsync(TaskChat chat, UserModel user)
        {
            var membership = await GetTaskChatMembershipAsync(chat.Id, user.Id);
            if (membership != null)
                return null;

            membership = new TaskChatMembership
            {
                Chat = chat,
                Participant = user
            };

            membership = (await _context.TaskChatMemberships.AddAsync(membership))?.Entity;
            await _context.SaveChangesAsync();

            return membership;
        }
    }
}
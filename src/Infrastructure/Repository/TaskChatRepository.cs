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
                var chat = chats.First(e => e.ChatId == chatMembership.Key).Chat;

                var taskChatBody = new TaskChatBody
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    ImageUrl = chat.Image == null ? null : $"{Constants.webPathToChatIcons}{chat.Image}",
                    Participants = chatMembership.Select(e => e.Participant.ToChatUserInfo()).ToList()
                };
                result.Add(taskChatBody);
            }

            return result;
        }

        public async Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId)
            => await _context.TaskChatMemberships
                .FirstOrDefaultAsync(e =>
                    e.ChatId == chatId && e.ParticipantId == userId);

        public async Task<TaskChatMembership?> AddOrGetUserChatHistoryAsync(TaskChat chat, UserModel user)
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
                .OrderBy(e => e.CreatedAtDate)
                .Where(e => e.CreatedAtDate > startedTime && e.ChatId == chatMembership.ChatId)
                .Take(count)
                .ToListAsync();

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
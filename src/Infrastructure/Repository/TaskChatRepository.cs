using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
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

        public async Task<TaskChat?> AddAsync(TaskModel task)
        {
            var chat = await GetByTaskAsync(task.Id);
            if (chat != null)
                return null;

            chat = new TaskChat
            {
                Task = task,
            };

            chat = (await _context.TaskChats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<TaskChat?> GetTaskChatWithParticipants(Guid taskId)
        {
            var chat = await _context.TaskChats
                .Include(e => e.Memberships)
                    .ThenInclude(e => e.Participant)
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

            return chat;
        }

        public async Task<List<TaskChatMembership>> GetTaskChatMemberships(Guid userId)
        {
            var chatIds = await _context.TaskChatMemberships
                .Where(e => e.ParticipantId == userId)
                .Select(e => e.ChatId)
                .ToListAsync();

            var chatMemberships = await _context.TaskChatMemberships
                .Include(e => e.Participant)
                .Where(e => chatIds.Contains(e.ChatId))
                .ToListAsync();

            return chatMemberships;
        }

        public async Task<TaskChatMembership?> GetUserChatHistoryAsync(Guid chatId, Guid userId)
            => await _context.TaskChatMemberships
                .FirstOrDefaultAsync(e =>
                    e.ChatId == chatId && e.ParticipantId == userId);

        public async Task<TaskChatMembership?> CreateOrGetUserChatHistoryAsync(TaskChat chat, UserModel user)
        {
            var userChatHistory = await GetUserChatHistoryAsync(chat.Id, user.Id);
            if (userChatHistory != null)
                return userChatHistory;

            userChatHistory = new TaskChatMembership
            {
                Chat = chat,
                Participant = user
            };

            userChatHistory = (await _context.TaskChatMemberships.AddAsync(userChatHistory))?.Entity;
            await _context.SaveChangesAsync();
            return userChatHistory;
        }

        public async Task<TaskChatMessage?> AddAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender)
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

        public async Task<TaskChat?> GetAsync(Guid id)
            => await _context.TaskChats.FindAsync(id);

        public async Task<TaskChat?> GetByTaskAsync(Guid taskId)
            => await _context.TaskChats
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

        public async Task<IEnumerable<TaskChatMessage>> GetLastMessages(TaskChatMembership chatHistory, DateTime startedTime, int count)
        {
            var messages = await _context.TaskChatMessages
                .OrderBy(e => e.CreatedAtDate)
                .Where(e => e.CreatedAtDate > startedTime && e.ChatId == chatHistory.ChatId)
                .Take(count)
                .ToListAsync();

            var lastMessage = messages.LastOrDefault();
            if (lastMessage != null)
            {
                chatHistory.DateLastViewing = lastMessage.CreatedAtDate;
                await _context.SaveChangesAsync();
            }

            return messages;
        }

        public async Task<TaskChatMembership?> GetTaskChatMembershipAsync(Guid chatId, Guid userId)
            => await _context.TaskChatMemberships
                .FirstOrDefaultAsync(e =>
                    e.ChatId == chatId && e.ParticipantId == userId);

        public async Task<TaskChatMembership?> AddAsync(TaskChat chat, UserModel user)
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
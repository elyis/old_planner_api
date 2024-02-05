using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
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

        public async Task<TaskChat?> AddAsync(TaskModel task)
        {
            var chat = await GetByTaskAsync(task.Id);
            if (chat != null)
                return null;

            chat = new TaskChat
            {
                Task = task,
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<UserChatHistory?> GetUserChatHistoryAsync(Guid chatId, Guid userId)
            => await _context.UserChatHistories
                .FirstOrDefaultAsync(e =>
                    e.ChatId == chatId && e.ParticipantId == userId);

        public async Task<UserChatHistory?> CreateOrGetUserChatHistoryAsync(TaskChat chat, UserModel user)
        {
            var userChatHistory = await GetUserChatHistoryAsync(chat.Id, user.Id);
            if (userChatHistory != null)
                return userChatHistory;

            userChatHistory = new UserChatHistory
            {
                Chat = chat,
                Participant = user
            };

            userChatHistory = (await _context.UserChatHistories.AddAsync(userChatHistory))?.Entity;
            await _context.SaveChangesAsync();
            return userChatHistory;
        }

        public async Task<ChatMessage?> AddAsync(CreateMessageBody messageBody, TaskChat chat, UserModel sender)
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

        public async Task<TaskChat?> GetAsync(Guid id)
            => await _context.Chats.FindAsync(id);

        public async Task<TaskChat?> GetByTaskAsync(Guid taskId)
            => await _context.Chats
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

        public async Task<IEnumerable<ChatMessage>> GetLastMessages(UserChatHistory chatHistory, DateTime startedTime, int count)
        {
            var messages = await _context.ChatMessages
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
    }
}
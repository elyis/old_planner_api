using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid SenderId { get; set; }
        public UserModel Sender { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public List<TaskAttachedMessage> Tasks { get; set; } = new();

        public CreateMessageBody ToCreateMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new CreateMessageBody
            {
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.webPathToChatPrivateAttachment}{Content}" : Content,
            };
        }

        public MessageBody ToMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new MessageBody
            {
                Id = Id,
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.webPathToChatPrivateAttachment}{Content}" : Content,
                Date = SentAt,
                SenderId = SenderId,
            };
        }
    }
}
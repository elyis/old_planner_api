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
        public DateTime CreatedAtDate { get; set; } = DateTime.UtcNow;


        public Guid ChatId { get; set; }
        public TaskChat Chat { get; set; }

        public Guid SenderId { get; set; }
        public UserModel Sender { get; set; }


        public MessageBody ToMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new MessageBody
            {
                Id = Id,
                Type = messageType,
                SenderId = SenderId,
                Date = CreatedAtDate,
                Content = messageType == MessageType.File ? $"{Constants.webPathToTaskChatAttachments}{Content}" : Content
            };
        }

        public CreateMessageBody ToCreateMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new CreateMessageBody
            {
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.webPathToTaskChatAttachments}{Content}" : Content,
                SenderId = SenderId
            };
        }
    }
}
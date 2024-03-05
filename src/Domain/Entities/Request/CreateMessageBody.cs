using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateMessageBody
    {
        [EnumDataType(typeof(MessageType))]
        public MessageType Type { get; set; }

        [Required]
        public string Content { get; set; }
    }

    public class SentMessage
    {
        public CreateMessageBody? MessageBody { get; set; }
        public Guid? LastMessageReadId { get; set; }
    }
}
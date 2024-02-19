using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class ChatMessageInfo
    {
        public Guid ChatId { get; set; }
        public ChatType ChatType { get; set; }
        public MessageBody Message { get; set; }
    }
}
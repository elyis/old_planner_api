using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class MessageBody
    {
        public Guid Id { get; set; }
        public MessageType Type { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid SenderId { get; set; }
    }
}
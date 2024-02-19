namespace old_planner_api.src.Domain.Entities.Response
{
    public class ChatBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public MessageBody? LastMessage { get; set; }

        public List<ChatUserInfo> Participants { get; set; } = new();
    }
}
namespace old_planner_api.src.Domain.Entities.Response
{
    public class TaskChatBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }

        public List<ChatUserInfo> Participants { get; set; } = new();
    }
}
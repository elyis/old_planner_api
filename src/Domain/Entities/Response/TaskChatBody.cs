namespace old_planner_api.src.Domain.Entities.Response
{
    public class TaskChatBody
    {
        public Guid Id { get; set; }

        public List<ChatUserInfo> Participants { get; set; } = new();
    }
}
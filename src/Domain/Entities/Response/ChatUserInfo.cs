using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class ChatUserInfo
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public string? ImageUrl { get; set; }
        public string? UserTag { get; set; }
        public AuthenticationMethod IdentifierType { get; set; }
    }
}
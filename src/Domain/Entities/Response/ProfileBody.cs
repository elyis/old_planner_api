using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class ProfileBody
    {
        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public UserRole Role { get; set; }
        public string? UrlIcon { get; set; }
        public string? UserTag { get; set; }
        public AuthenticationMethod IdentifierType { get; set; }
    }
}
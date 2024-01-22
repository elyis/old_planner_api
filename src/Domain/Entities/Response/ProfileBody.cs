using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class ProfileBody
    {
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string? UrlIcon { get; set; }
    }
}
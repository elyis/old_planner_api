namespace old_planner_api.src.Domain.Models
{
    public class UserMailCredentials
    {
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string EmailProvider { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public Guid UserId { get; set; }
        public UserModel User { get; set; }
    }
}
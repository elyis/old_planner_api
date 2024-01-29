namespace old_planner_api.src.Domain.Models
{
    public class BoardMember
    {
        public Board Board { get; set; }
        public Guid BoardId { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public string Role { get; set; }
    }
}
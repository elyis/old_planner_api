namespace old_planner_api.src.Domain.Models
{
    public class BoardColumnMember
    {
        public BoardColumn Column { get; set; }
        public Guid ColumnId { get; set; }
        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public string Role { get; set; }
    }
}
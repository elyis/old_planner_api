using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Domain.Models
{
    public class BoardColumn
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Board Board { get; set; }
        public Guid BoardId { get; set; }
        public List<BoardColumnMember> Members { get; set; } = new();

        public BoardColumnBody ToBoardColumnBody()
        {
            return new BoardColumnBody
            {
                Id = Id,
                Name = Name,
            };
        }
    }
}
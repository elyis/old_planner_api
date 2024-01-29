using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.Domain.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<TaskModel> Tasks { get; set; } = new();
        public List<BoardMember> Members { get; set; } = new();


        public BoardBody ToBoardBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class CreateBoardBody
    {
        [Required]
        public string Name { get; set; }
    }
}
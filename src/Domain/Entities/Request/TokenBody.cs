using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class TokenBody
    {
        [Required]
        public string Value { get; set; }
    }
}
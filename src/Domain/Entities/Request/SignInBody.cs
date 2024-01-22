using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class SignInBody
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
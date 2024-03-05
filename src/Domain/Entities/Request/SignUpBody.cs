using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Enums;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class SignUpBody
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public string Password { get; set; }

        [EnumDataType(typeof(AuthenticationMethod))]
        public AuthenticationMethod Method { get; set; }
    }
}
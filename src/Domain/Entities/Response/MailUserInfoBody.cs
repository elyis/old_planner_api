using Newtonsoft.Json;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class MailUserInfoBody
    {
        [JsonProperty("first_name")]
        public string FirstName { get;set;}

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }
    }
}
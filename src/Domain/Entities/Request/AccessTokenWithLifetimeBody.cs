using Newtonsoft.Json;

namespace old_planner_api.src.Domain.Entities.Request
{
    public class AccessTokenWithLifetimeBody
    {
        [JsonProperty("access_token")]
        public string AccessToken { get;set;}

        [JsonProperty("expires_in")]
        public int ExpiresIn { get;set;}
    }
}
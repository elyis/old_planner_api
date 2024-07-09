using Newtonsoft.Json;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.App.Service
{
    public class GoogleTokenService : IGoogleTokenService
    {
        private readonly string clientId;
        private readonly string clientSecret;

        public GoogleTokenService(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var requestUrl = "https://oauth2.googleapis.com/token";
            var client = new HttpClient();
            var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" }
        };

            var requestContent = new FormUrlEncodedContent(requestData);
            var response = await client.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseString);
                return tokenResponse;
            }

            throw new Exception("Failed to refresh token.");
        }
    }
}
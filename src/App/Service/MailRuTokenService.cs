using Newtonsoft.Json;
using old_planner_api.src.App.IService;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;

namespace old_planner_api.src.App.Service
{
    public class MailRuTokenService : IMailRuTokenService
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string redirectUri;
        private const string profileEndpoint = "https://oauth.mail.ru/userinfo";
        private const string tokenEndpoint = "https://oauth.mail.ru/token";

        public MailRuTokenService(
            string clientId,
            string clientSecret,
            string redirectUri)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUri = redirectUri;
        }

        public string GetAuthorizationUrl()
        {
            return $"https://oauth.mail.ru/login?client_id={clientId}&response_type=code&scope=userinfo&score=mail.imap&redirect_uri={redirectUri}&state=urn:ietf:wg:oauth:2.0:oob:auto";
        }

        public async Task<MailruTokenResponse?> GetTokenAsync(string code)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");

            var requestParams = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri }
            };

            var requestContent = new FormUrlEncodedContent(requestParams);
            var response = await client.PostAsync(tokenEndpoint, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<MailruTokenResponse>(responseString);
        }

        public async Task<AccessTokenWithLifetimeBody?> UpdateToken(string refresh_token)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");

            var requestParams = new Dictionary<string, string>
            {
                {"client_id", $"{clientId}"},
                {"grant_type", "refresh_token"},
                {"refresh_token", $"{refresh_token}"}
            };

            var requestContent = new FormUrlEncodedContent(requestParams);
            var response = await client.PostAsync(tokenEndpoint, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<AccessTokenWithLifetimeBody>(responseString);
        }

        public async Task<MailUserInfoBody?> GetUserInfo(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");

            var requestParams = new Dictionary<string, string>
            {
                {"access_token", $"{accessToken}"}
            };

            var requestContent = new FormUrlEncodedContent(requestParams);
            var response = await client.PostAsync(profileEndpoint, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<MailUserInfoBody>(responseString);
        }
    }
}
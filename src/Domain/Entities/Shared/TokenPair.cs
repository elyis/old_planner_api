namespace old_planner_api.src.Domain.Entities.Shared
{
    public class TokenPair
    {
        public TokenPair(string accessToken, string refreshToken)
        {
            AccessToken = "Bearer " + accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
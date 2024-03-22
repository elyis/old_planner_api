namespace webApiTemplate.src.Domain.Entities.Shared
{
    public class TokenInfo
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public Guid SessionId { get; set; }
    }
}
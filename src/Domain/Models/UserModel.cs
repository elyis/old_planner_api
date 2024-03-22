using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace old_planner_api.src.Domain.Models
{
    [Index(nameof(Identifier), IsUnique = true)]
    [Index(nameof(Token))]
    public class UserModel
    {
        public Guid Id { get; set; }

        [StringLength(256, MinimumLength = 3)]
        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string RoleName { get; set; }
        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }
        public string? Image { get; set; }
        public string? UserTag { get; set; }
        public string AuthenticationMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TaskChatMessage> ChatMessages { get; set; } = new();
        public List<TaskChatMembership> TaskChatMemberships { get; set; } = new();
        public List<ChatMessage> SentMessages { get; set; } = new();

        public List<ChatMembership> ChatMemberships { get; set; } = new();
        public List<UserSession> Sessions { get; set; } = new();

        public ProfileBody ToProfileBody()
        {
            return new ProfileBody
            {
                Nickname = Nickname,
                Role = Enum.Parse<UserRole>(RoleName),
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.webPathToProfileIcons}{Image}",
                UserTag = UserTag,
                Identifier = Identifier,
                IdentifierType = Enum.Parse<AuthenticationMethod>(AuthenticationMethod)
            };
        }

        public ChatUserInfo ToChatUserInfo()
        {
            return new ChatUserInfo
            {
                Id = Id,
                Nickname = Nickname,
                Identifier = Identifier,
                ImageUrl = Image == null ? null : $"{Constants.webPathToProfileIcons}{Image}",
                UserTag = UserTag,
                IdentifierType = Enum.Parse<AuthenticationMethod>(AuthenticationMethod)
            };
        }
    }
}
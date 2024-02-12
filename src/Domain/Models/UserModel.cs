using System.ComponentModel.DataAnnotations;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace old_planner_api.src.Domain.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Token))]
    public class UserModel
    {
        public Guid Id { get; set; }

        [StringLength(256, MinimumLength = 3)]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string RoleName { get; set; }
        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TaskChatMessage> ChatMessages { get; set; } = new();
        public List<TaskChatMembership> TaskChatMemberships { get; set; } = new();
        public List<ChatMessage> SentMessages { get; set; } = new();

        public List<ChatMembership> ChatMemberships { get; set; } = new();

        public ProfileBody ToProfileBody()
        {
            return new ProfileBody
            {
                Email = Email,
                Role = Enum.Parse<UserRole>(RoleName),
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.webPathToProfileIcons}{Image}",
            };
        }

        public ChatUserInfo ToChatUserInfo()
        {
            return new ChatUserInfo
            {
                Id = Id,
                Email = Email,
                ImageUrl = Image == null ? null : $"{Constants.webPathToProfileIcons}{Image}"
            };
        }
    }
}
using Domain.Common;

namespace Domain.Models
{
    public class Account : EntityBase
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public required string HashPassword { get; set; }
        public required string Email { get; set; }
        public string TimeZone { get; set; } = "Etc/UTC";
        public ICollection<Quest> Quests { get; set; } = [];
        public ICollection<QuestLabel> Labels { get; set; } = [];
        public UserProfile Profile { get; set; } = null!;

        public Account() { }

        public Account(int accountId, string hashPassword, string email, string timeZone)
        {
            Id = accountId;
            HashPassword = hashPassword;
            Email = email;
            TimeZone = timeZone;
        }
    }
}

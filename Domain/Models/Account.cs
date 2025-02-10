using Domain.Common;

namespace Domain.Models
{
    public class Account : EntityBase
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string HashPassword { get; set; }
        public required string Email { get; set; }
        public ICollection<QuestMetadata> Quests { get; set; } = new List<QuestMetadata>();

        public Account() { }

        public Account(int accountId, string username, string hashPassword, string email)
        {
            Id = accountId;
            Username = username;
            HashPassword = hashPassword;
            Email = email;
        }
    }
}

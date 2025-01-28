using Domain.Common;

namespace Domain.Models
{
    public class Account : BaseEntity
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string HashPassword { get; set; }
        public required string Email { get; set; }
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();

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

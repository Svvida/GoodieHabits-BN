using Domain.Common;
using Domain.Exceptions;

namespace Domain.Models
{
    public class Account : EntityBase
    {
        public int Id { get; set; }
        public string? Login { get; set; } = null;
        public string HashPassword { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string TimeZone { get; private set; } = "Etc/UTC";
        public ICollection<Quest> Quests { get; set; } = [];
        public ICollection<QuestLabel> Labels { get; set; } = [];
        public UserProfile Profile { get; set; } = null!;
        public ICollection<UserGoal> UserGoals { get; set; } = [];

        protected Account() { }

        private Account(string hashPassword, string email, string timeZone)
        {
            HashPassword = hashPassword;
            Email = email;
            TimeZone = timeZone;

            SetCreatedAt(DateTime.UtcNow);

            Profile = new UserProfile(this);
        }

        public static Account Create(string hashPassword, string email, string timeZone = "Etc/UTC")
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                throw new InvalidArgumentException("HashPassword cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidArgumentException("Email cannot be null or whitespace.");
            return new Account(hashPassword, email, timeZone);
        }

        public void UpdateLogin(string? login)
        {
            Login = login;
        }

        public void UpdateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidArgumentException("Email cannot be null or whitespace.");
            Email = email;
        }

        public void UpdateTimeZone(string? timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                throw new InvalidArgumentException("TimeZone cannot be null or whitespace.");
            TimeZone = timeZone;
        }

        public void UpdateHashPassword(string hashPassword)
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                throw new InvalidArgumentException("HashPassword cannot be null or whitespace.");
            HashPassword = hashPassword;
        }
    }
}

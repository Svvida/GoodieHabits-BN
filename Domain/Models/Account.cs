using System.Security.Cryptography;
using System.Text;
using Domain.Common;
using Domain.Exceptions;

namespace Domain.Models
{
    public class Account : EntityBase
    {
        public int Id { get; set; }
        public string? Login { get; private set; } = null;
        public string HashPassword { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string TimeZone { get; private set; } = "Etc/UTC";
        public string? ResetPasswordCode { get; private set; } = null;
        public DateTime? ResetPasswordCodeExpiresAt { get; private set; } = null;
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

        public int ExpireGoals(DateTime nowUtc)
        {
            int expiredCount = 0;
            foreach (var goal in UserGoals)
            {
                if (goal.Expire(nowUtc))
                    expiredCount++;
            }
            Profile.IncrementExpiredGoals(expiredCount);
            return expiredCount;
        }

        public int ResetQuests(DateTime nowUtc)
        {
            int resetCount = 0;
            foreach (var quest in Quests)
            {
                if (quest.ResetCompletedStatus(nowUtc))
                    resetCount++;
            }
            Profile.DecrementCompletedQuestsAfterReset(resetCount);
            return resetCount;
        }

        public string InitializePasswordReset(DateTime utcNow)
        {
            var codeBuilder = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                codeBuilder.Append(RandomNumberGenerator.GetInt32(0, 10));
            }

            var code = codeBuilder.ToString();

            ResetPasswordCode = code;
            ResetPasswordCodeExpiresAt = utcNow.AddMinutes(15);

            return code;
        }
    }
}

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
        public string? ResetPasswordCode { get; private set; } = null;
        public DateTime? ResetPasswordCodeExpiresAt { get; private set; } = null;
        public UserProfile Profile { get; set; } = null!;

        protected Account() { }

        private Account(string hashPassword, string email, string timeZone)
        {
            HashPassword = hashPassword;
            Email = email;

            SetCreatedAt(DateTime.UtcNow);

            Profile = new UserProfile(this, timeZone);
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

        public void UpdateHashPassword(string hashPassword)
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                throw new InvalidArgumentException("HashPassword cannot be null or whitespace.");
            HashPassword = hashPassword;
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

        public void ResetPassword(string hashPassword)
        {
            HashPassword = hashPassword;
            ResetPasswordCode = null;
            ResetPasswordCodeExpiresAt = null;
        }
    }
}

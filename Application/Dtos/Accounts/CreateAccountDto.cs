namespace Application.Dtos.Accounts
{
    public class CreateAccountDto
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.

        /// <summary>
        /// Password for the account.
        /// Must be at least 8 characters long and contain at least one uppercase, one lowercase, one number, and one special character (_ @ # -).
        /// </summary>
        public string Password { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.
    }
}

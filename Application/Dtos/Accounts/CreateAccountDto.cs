namespace Application.Dtos.Accounts
{
    public class CreateAccountDto
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Password for the account.
        /// Must be at least 8 characters long and contain at least one uppercase, one lowercase, one number, and one special character (_ @ # -).
        /// </summary>
        public required string Password { get; set; }
    }
}

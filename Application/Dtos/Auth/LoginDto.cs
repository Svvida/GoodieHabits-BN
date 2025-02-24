namespace Application.Dtos.Auth
{
    /// <summary>
    /// DTO for user login.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// User's login (username or email).
        /// </summary>
        public required string Login { get; set; }

        /// <summary>
        /// User's password.
        /// </summary>
        public required string Password { get; set; }
    }

}

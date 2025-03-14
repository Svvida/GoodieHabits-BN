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
        public string Login { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.

        /// <summary>
        /// User's password.
        /// </summary>
        public string Password { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.
    }

}

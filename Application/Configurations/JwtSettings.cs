namespace Application.Configurations
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public AccessTokenSettings AccessToken { get; set; } = new();
        public RefreshTokenSettings RefreshToken { get; set; } = new();
    }
}

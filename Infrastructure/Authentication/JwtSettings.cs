namespace Infrastructure.Authentication
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public AccessTokenSettings AccessToken { get; set; } = new();
        public RefreshTokenSettings RefreshToken { get; set; } = new();
    }

    public class AccessTokenSettings
    {
        public string Key { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }

    public class RefreshTokenSettings
    {
        public string Key { get; set; } = string.Empty;
        public int ExpirationDays { get; set; } = 60;
    }
}

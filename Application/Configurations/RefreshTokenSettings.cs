namespace Application.Configurations
{
    public class RefreshTokenSettings
    {
        public string Key { get; set; } = string.Empty;
        public int ExpirationDays { get; set; } = 60;
    }
}

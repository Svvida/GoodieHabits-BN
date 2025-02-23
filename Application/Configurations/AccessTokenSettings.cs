namespace Application.Configurations
{
    public class AccessTokenSettings
    {
        public string Key { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }
}

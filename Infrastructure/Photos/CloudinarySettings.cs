namespace Infrastructure.Photos
{
    public class CloudinarySettings
    {
        public const string SectionName = "CloudinarySettings";
        public string Url { get; set; } = null!;
        public string CloudName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
        public string AvatarsFolder { get; set; } = null!;
    }
}

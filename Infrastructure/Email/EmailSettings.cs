namespace Infrastructure.Email
{
    public class EmailSettings
    {
        public const string SectionName = "SmtpSettings";
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587; // Default SMTP port for TLS
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}

using Domain.Enums;

namespace Domain.Models
{
    public class Badge
    {
        public int Id { get; private set; }
        public BadgeTypeEnum Type { get; private set; }
        public string Text { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string ColorHex { get; private set; } = string.Empty;

        public ICollection<UserProfile_Badge> UserProfile_Badges { get; set; } = [];

        protected Badge() { }
        public Badge(int id, BadgeTypeEnum type, string text, string description, string colorHex)
        {
            Id = id;
            Type = type;
            Text = text;
            Description = description;
            ColorHex = colorHex;
        }
    }
}

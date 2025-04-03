using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class Badge : EntityBase
    {
        public int Id { get; set; }
        // Setter should be private to prevent external modification but for now leave it as public to avoid toubles with projection in repositories
        public BadgeTypeEnum Type { get; set; }
        public required string Text { get; set; }

        public ICollection<UserProfile_Badge> UserProfile_Badges { get; set; } = [];

        public Badge() { }
        public Badge(int badgeId, BadgeTypeEnum type, string text)
        {
            Id = badgeId;
            Type = type;
            Text = text;
        }
    }
}

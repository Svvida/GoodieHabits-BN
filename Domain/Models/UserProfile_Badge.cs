namespace Domain.Models
{
    public class UserProfile_Badge
    {
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = null!;
        public int BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;
        public DateTime EarnedAt { get; set; }

        public UserProfile_Badge() { }
        public UserProfile_Badge(int userProfileId, int badgeId, DateTime utcNow)
        {
            UserProfileId = userProfileId;
            BadgeId = badgeId;
            EarnedAt = utcNow;
        }

        public UserProfile_Badge(UserProfile userProfile, Badge badge, DateTime utcNow)
        {
            UserProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
            Badge = badge ?? throw new ArgumentNullException(nameof(badge));
            EarnedAt = utcNow;
        }
    }
}

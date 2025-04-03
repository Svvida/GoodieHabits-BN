namespace Domain.Models
{
    public class UserProfile_Badge
    {
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = null!;
        public int BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;
        public DateTime EarnedAt { get; set; } = DateTime.Now;

        public UserProfile_Badge() { }
        public UserProfile_Badge(int userProfileId, int badgeId)
        {
            UserProfileId = userProfileId;
            BadgeId = badgeId;
            EarnedAt = DateTime.Now;
        }
    }
}

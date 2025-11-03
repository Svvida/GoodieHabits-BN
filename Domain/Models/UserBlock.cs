namespace Domain.Models
{
    public class UserBlock
    {
        public int BlockerUserProfileId { get; private set; }
        public int BlockedUserProfileId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public UserProfile BlockerUserProfile { get; private set; } = null!;
        public UserProfile BlockedUserProfile { get; private set; } = null!;

        protected UserBlock() { }
        private UserBlock(int blockerUserProfileId, int blockedUserProfileId, DateTime nowUtc)
        {
            BlockerUserProfileId = blockerUserProfileId;
            BlockedUserProfileId = blockedUserProfileId;
            CreatedAt = DateTime.UtcNow;
        }

        public static UserBlock Create(int blockerUserProfileId, int blockedUserProfileId, DateTime nowUtc)
        {
            return new UserBlock(blockerUserProfileId, blockedUserProfileId, nowUtc);
        }
    }
}

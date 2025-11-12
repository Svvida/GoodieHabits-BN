using Domain.Exceptions;

namespace Domain.Models
{
    public class Friendship
    {
        public int UserProfileId1 { get; private set; }
        public int UserProfileId2 { get; private set; }
        public DateTime BecameFriendsAt { get; private set; }

        public UserProfile UserProfile1 { get; private set; } = null!;
        public UserProfile UserProfile2 { get; private set; } = null!;

        protected Friendship() { }

        private Friendship(int userProfileId1, int userProfileId2, DateTime becameFriendsAt)
        {
            UserProfileId1 = userProfileId1;
            UserProfileId2 = userProfileId2;
            BecameFriendsAt = becameFriendsAt;
        }

        public static Friendship Create(int userProfileId1, int userProfileId2, DateTime becameFriendsAt)
        {
            if (userProfileId1 == userProfileId2)
                throw new FriendshipException("A user cannot be friends with themselves.");
            return (userProfileId1 < userProfileId2)
                ? new Friendship(userProfileId1, userProfileId2, becameFriendsAt)
                : new Friendship(userProfileId2, userProfileId1, becameFriendsAt);
        }
    }
}

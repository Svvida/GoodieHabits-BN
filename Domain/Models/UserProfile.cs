namespace Domain.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int UserLevel { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int QuestsCompleted { get; set; } = 0;

        public Account Account { get; set; } = null!;

        public UserProfile() { }
        public UserProfile(int id, int accountId)
        {
            Id = id;
            AccountId = accountId;
        }
    }
}

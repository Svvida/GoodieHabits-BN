using Domain.Common;

namespace Domain.Models
{
    public class UserSeasonalQuest : BaseEntity
    {
        public int AccountId { get; set; }
        public int SeasonalQuestId { get; set; }

        public Account Account { get; set; } = null!;
        public SeasonalQuest SeasonalQuest { get; set; } = null!;

        public UserSeasonalQuest() { }

        public UserSeasonalQuest(int accountId, int seasonalQuestId)
        {
            AccountId = accountId;
            SeasonalQuestId = seasonalQuestId;
        }
    }

}

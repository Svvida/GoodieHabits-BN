using Domain.Enum;

namespace Infrastructure.Authorization
{
    public class QuestAuthorizationContext
    {
        public int QuestId { get; set; }
        public int AccountId { get; set; }
        public QuestTypeEnum QuestType { get; set; }
    }
}

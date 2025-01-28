using Domain.Enum;

namespace Domain.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public QuestType QuestType { get; set; }
        public int AccountId { get; set; }

        public Account Account { get; set; } = null!;

        public Quest() { }
        public Quest(int id, QuestType type, int accountId)
        {
            Id = id;
            QuestType = type;
            AccountId = accountId;
        }
    }
}

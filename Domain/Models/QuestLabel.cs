using Domain.Common;

namespace Domain.Models
{
    public class QuestLabel : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public required string Value { get; set; }
        public required string BackgroundColor { get; set; }
        public required string TextColor { get; set; }

        public Account Account { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = null!;

        public QuestLabel() { }
        public QuestLabel(int id, int accountId, string value, string backgroundColor, string textColor)
        {
            Id = id;
            AccountId = accountId;
            Value = value;
            BackgroundColor = backgroundColor;
            TextColor = textColor;
        }
    }
}

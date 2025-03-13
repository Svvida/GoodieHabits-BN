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
        public ICollection<QuestMetadata_QuestLabel> QuestMetadataRelations { get; set; } = null!;

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

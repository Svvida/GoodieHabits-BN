namespace Domain.Models
{
    public class QuestMetadata_QuestLabel
    {
        public int QuestMetadataId { get; set; }
        public QuestMetadata QuestMetadata { get; set; } = null!;

        public int QuestLabelId { get; set; }
        public QuestLabel QuestLabel { get; set; } = null!;

        public QuestMetadata_QuestLabel() { }

        public QuestMetadata_QuestLabel(int questMetadataId, int questLabelId)
        {
            QuestMetadataId = questMetadataId;
            QuestLabelId = questLabelId;
        }
    }
}

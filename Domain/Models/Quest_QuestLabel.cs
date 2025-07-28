namespace Domain.Models
{
    public class Quest_QuestLabel
    {
        public int QuestId { get; set; }
        public Quest Quest { get; set; } = null!;

        public int QuestLabelId { get; set; }
        public QuestLabel QuestLabel { get; set; } = null!;

        public Quest_QuestLabel() { }
        public Quest_QuestLabel(int questId, int questLabelId)
        {
            QuestId = questId;
            QuestLabelId = questLabelId;
        }
        public Quest_QuestLabel(Quest quest, int labelId)
        {
            Quest = quest;
            QuestLabelId = labelId;
        }
    }
}

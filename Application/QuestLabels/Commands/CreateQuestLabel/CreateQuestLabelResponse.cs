namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public record CreateQuestLabelResponse(
        int Id,
        string Value,
        string BackgroundColor
    );
}

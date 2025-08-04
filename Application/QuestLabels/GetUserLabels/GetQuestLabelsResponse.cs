using Application.QuestLabels.Dtos;

namespace Application.QuestLabels.GetUserLabels
{
    public record GetQuestLabelsResponse(IEnumerable<QuestLabelDto> QuestLabels);
}

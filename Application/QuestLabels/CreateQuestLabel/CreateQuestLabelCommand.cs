using MediatR;

namespace Application.QuestLabels.CreateQuestLabel
{
    public record CreateQuestLabelCommand(string Value, string BackgroundColor, int AccountId) : IRequest<CreateQuestLabelResponse>;
}

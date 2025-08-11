using MediatR;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public record CreateQuestLabelCommand(string Value, string BackgroundColor, int AccountId) : IRequest<CreateQuestLabelResponse>;
}

using Application.QuestLabels.Queries.GetUserLabels;
using MediatR;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public record CreateQuestLabelCommand(CreateQuestLabelDto CreateDto) : IRequest<GetQuestLabelDto>;
}

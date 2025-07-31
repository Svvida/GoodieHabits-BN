using Application.QuestLabels.Queries.GetUserLabels;
using MediatR;

namespace Application.QuestLabels.Commands.PatchQuestLabel
{
    public record PatchQuestLabelCommand(PatchQuestLabelDto PatchDto) : IRequest<GetQuestLabelDto>;
}

using MediatR;

namespace Application.QuestLabels.GetUserLabels
{
    public record GetUserLabelsQuery(int AccountId) : IRequest<GetQuestLabelsResponse>;
}

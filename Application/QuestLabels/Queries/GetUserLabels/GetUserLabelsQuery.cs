using Application.QuestLabels.Dtos;
using MediatR;

namespace Application.QuestLabels.Queries.GetUserLabels
{
    public record GetUserLabelsQuery(int AccountId) : IRequest<IEnumerable<QuestLabelDto>>;
}

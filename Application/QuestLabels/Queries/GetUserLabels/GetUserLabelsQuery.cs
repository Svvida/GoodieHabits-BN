using Application.Common.Interfaces;
using Application.QuestLabels.Dtos;

namespace Application.QuestLabels.Queries.GetUserLabels
{
    public record GetUserLabelsQuery(int AccountId) : IQuery<IEnumerable<QuestLabelDto>>;
}

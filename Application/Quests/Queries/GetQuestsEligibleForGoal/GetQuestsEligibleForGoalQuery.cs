using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetQuestsEligibleForGoal
{
    public record GetQuestsEligibleForGoalQuery(int UserProfileId, CancellationToken CancellationToken) : IQuery<IEnumerable<QuestDetailsDto>>;
}

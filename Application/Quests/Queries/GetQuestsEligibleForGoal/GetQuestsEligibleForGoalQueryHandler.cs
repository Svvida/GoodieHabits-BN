using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetQuestsEligibleForGoal
{
    public class GetQuestsEligibleForGoalQueryHandler(IUnitOfWork unitOfWork,
        IQuestMapper questMapper) : IRequestHandler<GetQuestsEligibleForGoalQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsEligibleForGoalQuery request, CancellationToken cancellationToken = default)
        {
            DateOnly today = DateOnly.FromDateTime(SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc());
            var quests = await unitOfWork.Quests.GetQuestEligibleForGoalAsync(request.UserProfileId, today, cancellationToken).ConfigureAwait(false);
            return quests.Select(questMapper.MapToDto);
        }
    }
}

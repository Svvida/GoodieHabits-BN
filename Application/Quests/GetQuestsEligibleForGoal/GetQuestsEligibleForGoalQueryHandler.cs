using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.GetQuestsEligibleForGoal
{
    public class GetQuestsEligibleForGoalQueryHandler(IUnitOfWork unitOfWork,
        IQuestMappingService questMapper) : IRequestHandler<GetQuestsEligibleForGoalQuery, IEnumerable<QuestDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMapper = questMapper;

        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsEligibleForGoalQuery request, CancellationToken cancellationToken = default)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var quests = await _unitOfWork.Quests.GetQuestEligibleForGoalAsync(request.AccountId, nowUtc, cancellationToken).ConfigureAwait(false);
            return quests.Select(_questMapper.MapToDto);
        }
    }
}

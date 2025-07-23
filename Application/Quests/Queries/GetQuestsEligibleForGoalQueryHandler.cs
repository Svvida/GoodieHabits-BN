using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries
{
    public class GetQuestsEligibleForGoalQueryHandler(IUnitOfWork unitOfWork,
        IQuestMappingService questMapper) : IRequestHandler<GetQuestsEligibleForGoalQuery, IEnumerable<BaseGetQuestDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMapper = questMapper;

        public async Task<IEnumerable<BaseGetQuestDto>> Handle(GetQuestsEligibleForGoalQuery request, CancellationToken cancellationToken = default)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var quests = await _unitOfWork.Quests.GetQuestEligibleForGoalAsync(request.AccountId, nowUtc, cancellationToken).ConfigureAwait(false);
            return quests.Select(_questMapper.MapToDto);
        }
    }
}

using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Queries
{
    public class GetQuestsByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMappingService questMapper)
        : IRequestHandler<GetQuestsByTypeQuery, IEnumerable<BaseGetQuestDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMapper = questMapper;

        public async Task<IEnumerable<BaseGetQuestDto>> Handle(GetQuestsByTypeQuery request, CancellationToken cancellationToken = default)
        {
            var quests = await _unitOfWork.Quests.GetQuestsByTypeForDisplayAsync(request.AccountId, request.QuestType, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(_questMapper.MapToDto);
        }
    }
}

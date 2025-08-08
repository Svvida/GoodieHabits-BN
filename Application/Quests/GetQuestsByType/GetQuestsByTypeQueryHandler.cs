using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.GetQuestsByType
{
    public class GetQuestsByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMapper)
        : IRequestHandler<GetQuestsByTypeQuery, IEnumerable<QuestDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMapper _questMapper = questMapper;

        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsByTypeQuery request, CancellationToken cancellationToken = default)
        {
            var quests = await _unitOfWork.Quests.GetQuestsByTypeForDisplayAsync(request.AccountId, request.QuestType, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(_questMapper.MapToDto);
        }
    }
}

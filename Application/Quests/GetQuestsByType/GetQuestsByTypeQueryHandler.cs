using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.GetQuestsByType
{
    public class GetQuestsByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMapper)
        : IRequestHandler<GetQuestsByTypeQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsByTypeQuery request, CancellationToken cancellationToken = default)
        {
            var quests = await unitOfWork.Quests.GetQuestsByTypeForDisplayAsync(request.AccountId, request.QuestType, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(questMapper.MapToDto);
        }
    }
}

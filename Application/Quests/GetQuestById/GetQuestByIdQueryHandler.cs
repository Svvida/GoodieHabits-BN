using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.GetQuestById
{
    public class GetQuestByIdQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMappingService)
        : IRequestHandler<GetQuestByIdQuery, QuestDetailsDto?>
    {
        public async Task<QuestDetailsDto?> Handle(GetQuestByIdQuery request, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdAsync(request.QuestId, request.QuestType, true, cancellationToken);
            if (quest is null)
                return null;

            return questMappingService.MapToDto(quest);
        }
    }
}

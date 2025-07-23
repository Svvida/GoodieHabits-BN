using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.Queries
{
    public class GetQuestByIdQueryHandler(IUnitOfWork unitOfWork, IQuestMappingService questMappingService) : IRequestHandler<GetQuestByIdQuery, BaseGetQuestDto?>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMappingService = questMappingService;

        public async Task<BaseGetQuestDto?> Handle(GetQuestByIdQuery request, CancellationToken cancellationToken = default)
        {
            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(request.QuestId, request.QuestType, true, cancellationToken);
            if (quest is null)
                return null;

            return _questMappingService.MapToDto(quest);
        }
    }
}

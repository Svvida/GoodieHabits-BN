using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.UserGoals.Queries.GetActiveGoalByType
{
    public class GetActiveGoalByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMappingService questMappingService) : IRequestHandler<GetActiveGoalByTypeQuery, BaseGetQuestDto?>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMappingService = questMappingService;
        public async Task<BaseGetQuestDto?> Handle(GetActiveGoalByTypeQuery request, CancellationToken cancellationToken)
        {
            var goal = await _unitOfWork.UserGoals.GetUserActiveGoalByTypeAsync(request.AccountId, request.GoalType, cancellationToken).ConfigureAwait(false);
            if (goal is null)
                return null;

            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(goal.QuestId, goal.Quest.QuestType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {goal.QuestId} not found.");

            return _questMappingService.MapToDto(quest);
        }
    }
}

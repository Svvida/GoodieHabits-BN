using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.UserGoals.GetActiveGoalByType
{
    public class GetActiveGoalByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMappingService) : IRequestHandler<GetActiveGoalByTypeQuery, QuestDetailsDto?>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMapper _questMappingService = questMappingService;
        public async Task<QuestDetailsDto?> Handle(GetActiveGoalByTypeQuery request, CancellationToken cancellationToken)
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

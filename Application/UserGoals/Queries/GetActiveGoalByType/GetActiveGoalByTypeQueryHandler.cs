using Application.Quests;
using Application.Quests.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.UserGoals.Queries.GetActiveGoalByType
{
    public class GetActiveGoalByTypeQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMappingService, IClock clock) : IRequestHandler<GetActiveGoalByTypeQuery, QuestDetailsDto?>
    {
        public async Task<QuestDetailsDto?> Handle(GetActiveGoalByTypeQuery request, CancellationToken cancellationToken)
        {
            var goal = await unitOfWork.UserGoals.GetUserActiveGoalByTypeAsync(request.UserProfileId, request.GoalType, cancellationToken).ConfigureAwait(false);
            if (goal is null)
                return null;

            var quest = await unitOfWork.Quests.GetQuestByIdAsync(goal.QuestId, goal.UserProfileId, asNoTracking: false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {goal.QuestId} not found.");

            var today = quest.UserProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            return questMappingService.MapToDto(quest, today);
        }
    }
}

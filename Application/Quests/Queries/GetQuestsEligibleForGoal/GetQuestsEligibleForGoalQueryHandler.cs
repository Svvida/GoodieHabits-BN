using Application.Quests.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetQuestsEligibleForGoal
{
    public class GetQuestsEligibleForGoalQueryHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IClock clock) : IRequestHandler<GetQuestsEligibleForGoalQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsEligibleForGoalQuery request, CancellationToken cancellationToken = default)
        {
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {request.UserProfileId} not found.");

            // The user's local date, not the server's UTC one: a quest that ends today must stay eligible
            // until the user's day is actually over.
            var today = userProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            var quests = await unitOfWork.Quests
                .GetQuestEligibleForGoalAsync(request.UserProfileId, today, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(quest => questMapper.MapToDto(quest, today)).ToList();
        }
    }
}

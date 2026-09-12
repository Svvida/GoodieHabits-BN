using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetQuestById
{
    public class GetQuestByIdQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMapper, IClock clock)
        : IRequestHandler<GetQuestByIdQuery, QuestDetailsDto?>
    {
        public async Task<QuestDetailsDto?> Handle(GetQuestByIdQuery request, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests
                .GetQuestByIdAsync(request.QuestId, request.UserProfileId, asNoTracking: true, cancellationToken)
                .ConfigureAwait(false);

            if (quest is null)
                return null;

            var today = quest.UserProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            return questMapper.MapToDto(quest, today);
        }
    }
}

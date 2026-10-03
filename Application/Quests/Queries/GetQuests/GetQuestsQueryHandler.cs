using Application.Quests.Dtos;
using Application.Quests.Utilities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetQuests
{
    public class GetQuestsQueryHandler(IUnitOfWork unitOfWork, IQuestMapper questMapper, IClock clock)
        : IRequestHandler<GetQuestsQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetQuestsQuery request, CancellationToken cancellationToken = default)
        {
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {request.UserProfileId} not found.");

            var today = userProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            PeriodUnitEnum? unit = request.Unit is null
                ? null
                : QuestScheduleTranslator.ParseUnit(request.Unit);

            QuestTypeEnum? legacyType = null;
            if (request.LegacyType is not null)
            {
                if (!Enum.TryParse<QuestTypeEnum>(request.LegacyType, ignoreCase: true, out var parsed))
                    throw new InvalidArgumentException($"'{request.LegacyType}' is not a valid quest type.");

                legacyType = parsed;
            }

            var quests = await unitOfWork.Quests
                .GetQuestsForDisplayAsync(request.UserProfileId, unit, legacyType, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(quest => questMapper.MapToDto(quest, today, includeLegacyType: true)).ToList();
        }
    }
}

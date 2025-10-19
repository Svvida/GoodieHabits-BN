using Application.Badges;
using Application.Common;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestCommandHandler<TCommand, TResponse>(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : IRequestHandler<TCommand, TResponse>
        where TCommand : CreateQuestCommand, IRequest<TResponse> where TResponse : QuestDetailsDto
    {

        public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileWithBadgesAsync(command.UserProfileId, cancellationToken)
                ?? throw new NotFoundException($"User Profile with ID: {command.UserProfileId} not found.");

            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

            var quest = Quest.Create(
                title: command.Title,
                userProfile: userProfile,
                questType: command.QuestType,
                description: command.Description,
                priority: EnumHelper.ParseNullable<PriorityEnum>(command.Priority),
                emoji: command.Emoji,
                startDate: command.StartDate,
                endDate: command.EndDate,
                difficulty: EnumHelper.ParseNullable<DifficultyEnum>(command.Difficulty),
                scheduledTime: command.ScheduledTime,
                labelIds: command.Labels,
                nowUtc: nowUtc
            );

            await HandleQuestSpecificsAsync(quest, command, cancellationToken).ConfigureAwait(false);

            await unitOfWork.Quests.AddAsync(quest, cancellationToken).ConfigureAwait(false);

            if (quest.IsRepeatable())
            {
                quest.SetNextResetAt();
                quest.InitializeOccurrences(nowUtc);
                quest.RecalculateStatistics(nowUtc);
            }

            userProfile.UpdateAfterQuestCreation();

            await badgeAwardingService.CheckAndAwardBadgesAsync(BadgeTriggerEnum.QuestCreated, userProfile, null, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var questDetailsDto = questMappingService.MapToDto(quest);

            return (TResponse)questDetailsDto;
        }

        /// <summary>
        /// A hook for derived classes to implement quest-type-specific logic.
        /// </summary>
        protected virtual Task HandleQuestSpecificsAsync(Quest quest, TCommand command, CancellationToken cancellationToken)
        {
            // This method can be overridden in derived handlers to handle specific quest types
            return Task.CompletedTask;
        }
    }
}

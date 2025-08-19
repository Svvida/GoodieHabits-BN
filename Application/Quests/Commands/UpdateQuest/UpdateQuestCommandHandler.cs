using Application.Common;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestCommandHandler<TCommand, TResponse>(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : IRequestHandler<TCommand, TResponse>
        where TCommand : UpdateQuestCommand, IRequest<TResponse> where TResponse : QuestDetailsDto
    {
        public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdForUpdateAsync(command.QuestId, command.QuestType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {command.QuestId} not found.");

            quest.UpdateDescription(command.Description);

            quest.UpdatePriority(EnumHelper.ParseNullable<PriorityEnum>(command.Priority));

            quest.UpdateEmoji(command.Emoji);

            quest.UpdateScheduledTime(command.ScheduledTime);

            quest.UpdateDifficulty(EnumHelper.ParseNullable<DifficultyEnum>(command.Difficulty));

            quest.UpdateDates(command.StartDate, command.EndDate);

            quest.SetLabels(command.Labels);

            await HandleQuestSpecificsAsync(quest, command, cancellationToken).ConfigureAwait(false);

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (quest.IsRepeatable())
            {
                quest.SetNextResetAt();
                quest.GenerateMissingOccurrences(now);
            }

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

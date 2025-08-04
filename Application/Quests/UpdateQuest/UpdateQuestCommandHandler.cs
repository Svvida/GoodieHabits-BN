using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.UpdateQuest
{
    public class UpdateQuestCommandHandler<TCommand, TResponse>(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService)
        : IRequestHandler<TCommand, TResponse>
        where TCommand : UpdateQuestCommand, IRequest<TResponse> where TResponse : QuestDetailsDto
    {

        public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdAsync(command.QuestId, command.QuestType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {command.QuestId} not found.");

            quest.UpdateDescription(command.Description);

            quest.UpdatePriority(Enum.TryParse<PriorityEnum>(command.Priority, true, out var priority) ? priority : null);

            quest.UpdateEmoji(command.Emoji);

            quest.UpdateScheduledTime(command.ScheduledTime);

            quest.UpdateDifficulty(Enum.TryParse<DifficultyEnum>(command.Difficulty, true, out var difficulty) ? difficulty : null);

            quest.UpdateDates(command.StartDate, command.EndDate);

            quest.SetLabels(command.Labels);

            await HandleQuestSpecificsAsync(quest, command, cancellationToken).ConfigureAwait(false);

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (quest.IsRepeatable())
            {
                quest.SetNextResetAt(questResetService);
                if (await unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, now, cancellationToken).ConfigureAwait(false) is null)
                {
                    var generatedOccurrences = await questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken).ConfigureAwait(false);
                    if (generatedOccurrences.Count != 0)
                        quest.AddOccurrences(generatedOccurrences);
                }
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

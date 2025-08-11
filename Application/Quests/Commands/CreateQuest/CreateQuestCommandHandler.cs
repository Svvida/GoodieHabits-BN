using Application.Common;
using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestCommandHandler<TCommand, TResponse>(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestMapper questMappingService)
        : IRequestHandler<TCommand, TResponse>
        where TCommand : CreateQuestCommand, IRequest<TResponse> where TResponse : QuestDetailsDto
    {

        public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetAccountWithProfileAsync(command.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Account with ID: {command.AccountId} not found.");

            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

            var quest = Quest.Create(
                title: command.Title,
                account: account,
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
            }

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            quest.ClearDomainEvents();

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

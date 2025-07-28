using Application.Common;
using Application.Dtos.Quests;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService,
        IQuestResetService questResetService) : IRequestHandler<CreateQuestCommand, BaseGetQuestDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPublisher _publisher = publisher;
        private readonly IQuestMappingService _questMappingService = questMappingService;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator = questOccurrenceGenerator;

        public async Task<BaseGetQuestDto> Handle(CreateQuestCommand command, CancellationToken cancellationToken)
        {
            var createDto = command.CreateDto;

            var account = await _unitOfWork.Accounts.GetAccountWithProfileInfoAsync(createDto.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Account with ID: {createDto.AccountId} not found.");

            var quest = Quest.Create(
                title: createDto.Title,
                account: account,
                questType: createDto.QuestType,
                description: createDto.Description,
                priority: EnumHelper.ParseNullable<PriorityEnum>(createDto.Priority),
                emoji: createDto.Emoji,
                startDate: createDto.StartDate,
                endDate: createDto.EndDate,
                difficulty: EnumHelper.ParseNullable<DifficultyEnum>(createDto.Difficulty),
                scheduledTime: createDto.ScheduledTime,
                labelIds: createDto.Labels
            );

            // Handle specific quest types
            switch (quest.QuestType)
            {
                case QuestTypeEnum.Weekly:
                {
                    var createWeeklyDto = (CreateWeeklyQuestDto)createDto;
                    quest.SetWeekdays(createWeeklyDto.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d)));
                    break;
                }
                case QuestTypeEnum.Monthly:
                {
                    var createMonthlyDto = (CreateMonthlyQuestDto)createDto;
                    quest.SetMonthlyDays(createMonthlyDto.StartDay, createMonthlyDto.EndDay);
                    break;
                }
                case QuestTypeEnum.Seasonal:
                {
                    var createSeasonalDto = (CreateSeasonalQuestDto)createDto;
                    quest.SetSeason(Enum.Parse<SeasonEnum>(createSeasonalDto.Season));
                    break;
                }
            }

            await _unitOfWork.Quests.AddAsync(quest, cancellationToken).ConfigureAwait(false);

            if (quest.IsRepeatable())
            {
                quest.SetNextResetAt(questResetService);
                quest.AddOccurrences(await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken).ConfigureAwait(false));
            }

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await _publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            quest.ClearDomainEvents();

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return _questMappingService.MapToDto(quest);
        }
    }
}

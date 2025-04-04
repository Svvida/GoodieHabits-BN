using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IQuestRepository _questRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;
        private readonly IQuestWeekdaysHandler _questWeekdaysHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;
        private readonly IQuestLabelRepository _questLabelRepository;
        private readonly IQuestResetService _questResetService;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserProfileRepository _userProfileRepository;

        public QuestService(
            IQuestRepository repository,
            IQuestLabelsHandler questLabelsHandler,
            IQuestWeekdaysHandler questWeekdaysHandler,
            IMapper mapper,
            ILogger<QuestService> logger,
            IQuestLabelRepository questLabelRepository,
            IQuestResetService questResetService,
            IAccountRepository accountRepository,
            IUserProfileRepository userProfileRepository)
        {
            _questRepository = repository;
            _questLabelsHandler = questLabelsHandler;
            _questWeekdaysHandler = questWeekdaysHandler;
            _mapper = mapper;
            _logger = logger;
            _questLabelRepository = questLabelRepository;
            _questResetService = questResetService;
            _accountRepository = accountRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<BaseGetQuestDto?> GetUserQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, questType, cancellationToken);

            if (quest is null)
                return null;

            return MapToDto(quest);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, questType, cancellationToken)
                .ConfigureAwait(false);
            return quests.Select(MapToDto);
        }
        public async Task<int> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            // Check if the account is owner of the labels
            foreach (var labelId in createDto.Labels)
            {
                var isOwner = await _questLabelRepository.IsLabelOwnedByUserAsync(labelId, createDto.AccountId, cancellationToken)
                    .ConfigureAwait(false);
                if (!isOwner)
                    throw new ForbiddenException($"Label with ID: {labelId} does not belong to the user.");
            }

            var quest = _mapper.Map<Quest>(createDto);
            _logger.LogInformation("Quest created after mapping: {@quest}", quest);

            await _questRepository.AddQuestAsync(quest, cancellationToken);

            // For now keeping fetch-update logic to increment the total quests count and keep tracking by EF Core
            // Later we can consider using a more efficient approach like ExecuteUpdate
            var userProfile = await _userProfileRepository.GetByAccountIdAsync(createDto.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with account ID: {createDto.AccountId} not found");

            userProfile.TotalQuests++;

            await _userProfileRepository.UpdateAsync(userProfile, cancellationToken).ConfigureAwait(false);

            return quest.Id;
        }

        public async Task UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Type of updateDto from controller: {@updateDto}", updateDto);
            var existingQuest = await _questRepository.GetQuestByIdAsync(updateDto.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {updateDto.Id} not found");

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            existingQuest = _mapper.Map(updateDto, existingQuest);
            _logger.LogInformation("Quest updated after mapping: {@quest}", existingQuest);

            existingQuest = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);

            if (questType == QuestTypeEnum.Weekly)
            {
                var weeklyUpdateDto = (UpdateWeeklyQuestDto)updateDto;
                existingQuest = _questWeekdaysHandler.HandleUpdateWeekdays(existingQuest, weeklyUpdateDto);
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);
            }

            if (questType == QuestTypeEnum.Monthly)
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(BaseQuestCompletionPatchDto patchDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(patchDto.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {patchDto.Id} not found");

            if (existingQuest.Account is null || string.IsNullOrWhiteSpace(existingQuest.Account.TimeZone))
            {
                _logger.LogError("Account {AccountId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.", existingQuest.AccountId, existingQuest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {existingQuest.Id}.");
            }

            bool justCompleted = !existingQuest.IsCompleted && patchDto.IsCompleted;
            bool shouldIncrementCount = false;
            Instant nowUtc = SystemClock.Instance.GetCurrentInstant();

            if (justCompleted)
            {
                DateTimeZone? userTimeZone = DateTimeZoneProviders.Tzdb[existingQuest.Account.TimeZone]
                    ?? throw new NotFoundException($"Timezone with ID: {existingQuest.Account.TimeZone} not found");

                LocalDateTime nowUserLocal = nowUtc.InZone(userTimeZone).LocalDateTime;

                if (!existingQuest.LastCompletedAt.HasValue)
                {
                    shouldIncrementCount = true;
                }
                else
                {
                    Instant lastCompletedAtUtc = Instant.FromDateTimeUtc(DateTime.SpecifyKind(existingQuest.LastCompletedAt.Value, DateTimeKind.Utc));
                    LocalDateTime lastCompletedAtUserLocal = lastCompletedAtUtc.InZone(userTimeZone).LocalDateTime;

                    if (lastCompletedAtUserLocal.Date < nowUserLocal.Date)
                        shouldIncrementCount = true;
                    else
                        _logger.LogInformation($"Quest {existingQuest.Id} already completed today: {nowUserLocal} in user's timezone {existingQuest.Account.TimeZone}. Last Completion: {lastCompletedAtUserLocal}");

                }

                existingQuest.LastCompletedAt = nowUtc.ToDateTimeUtc();
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);
            }

            existingQuest = _mapper.Map(patchDto, existingQuest);

            _logger.LogInformation("Completed quest after mapping: {@existingQuest}", existingQuest);

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);

            if (shouldIncrementCount)
            {
                var userProfile = await _userProfileRepository.GetByAccountIdAsync(existingQuest.AccountId, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"User profile with account ID: {existingQuest.AccountId} not found");

                userProfile.CompletedQuests++;
                await _userProfileRepository.UpdateAsync(userProfile, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Incremented CompletedQuests count for Account {AccountId}", existingQuest.AccountId);
            }
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(
            int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            DateTimeZone? userTimezone = DateTimeZoneProviders.Tzdb[account.TimeZone]
                ?? throw new InvalidArgumentException($"Invalid timezone: {account.TimeZone}");

            Instant nowUtc = SystemClock.Instance.GetCurrentInstant();
            LocalDateTime nowLocal = nowUtc.InZone(userTimezone).LocalDateTime;

            DateTime todayStart = nowLocal.Date.AtStartOfDayInZone(userTimezone).ToDateTimeUtc();
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);

            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason();

            var quests = await _questRepository.GetActiveQuestsAsync(accountId, todayStart, todayEnd, currentSeason, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Quests before mapping: {@quests}", quests);

            var mappedQuests = quests.Select(MapToDto);

            _logger.LogInformation("Quests after mapping: {@mappedQuests}", mappedQuests);

            return mappedQuests;
        }

        public async Task DeleteQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            await _questRepository.DeleteQuestByIdAsync(questId, cancellationToken).ConfigureAwait(false);
        }

        private BaseGetQuestDto MapToDto(Quest quest)
        {
            return quest.QuestType switch
            {
                QuestTypeEnum.OneTime => _mapper.Map<GetOneTimeQuestDto>(quest),
                QuestTypeEnum.Daily => _mapper.Map<GetDailyQuestDto>(quest),
                QuestTypeEnum.Weekly => _mapper.Map<GetWeeklyQuestDto>(quest),
                QuestTypeEnum.Monthly => _mapper.Map<GetMonthlyQuestDto>(quest),
                QuestTypeEnum.Seasonal => _mapper.Map<GetSeasonalQuestDto>(quest),
                _ => throw new InvalidArgumentException("Invalid quest type")
            };
        }
    }
}


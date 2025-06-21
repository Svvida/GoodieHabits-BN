using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Quests;
using Application.Models;
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
        private readonly IUserGoalRepository _userGoalRepository;
        private readonly IQuestOccurrenceRepository _questOccurrenceRepository;
        private readonly IQuestStatisticsService _questStatisticsService;
        private readonly IQuestRewardCalculator _questRewardCalculator;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator;

        public QuestService(
            IQuestRepository repository,
            IQuestLabelsHandler questLabelsHandler,
            IQuestWeekdaysHandler questWeekdaysHandler,
            IMapper mapper,
            ILogger<QuestService> logger,
            IQuestLabelRepository questLabelRepository,
            IQuestResetService questResetService,
            IAccountRepository accountRepository,
            IUserProfileRepository userProfileRepository,
            IUserGoalRepository userGoalRepository,
            IQuestOccurrenceRepository questOccurrenceRepository,
            IQuestStatisticsService questStatisticsService,
            IQuestRewardCalculator questRewardCalculator,
            IQuestOccurrenceGenerator questOccurrenceGenerator)
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
            _userGoalRepository = userGoalRepository;
            _questOccurrenceRepository = questOccurrenceRepository;
            _questStatisticsService = questStatisticsService;
            _questRewardCalculator = questRewardCalculator;
            _questOccurrenceGenerator = questOccurrenceGenerator;
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
        public async Task<BaseGetQuestDto> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
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
            _logger.LogDebug("Quest created after mapping: {@quest}", quest);

            if (quest.IsRepeatable())
                quest.Statistics = new QuestStatistics();

            await _questRepository.AddQuestAsync(quest, cancellationToken);

            var createdQuest = await _questRepository.GetQuestByIdAsync(quest.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest not found after creation.");

            // Handle NextResetAt after creation to get access to user's timezone. Value will be saved with occurrences
            if (createdQuest.IsRepeatable())
            {
                createdQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(createdQuest);
                await _questRepository.UpdateQuestAsync(createdQuest, cancellationToken).ConfigureAwait(false);
                var occurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesAsync(createdQuest, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("Initial occurrences created: {@occurrences}", occurrences);
            }

            // For now keeping fetch-update logic to increment the total quests count and keep tracking by EF Core
            // Later we can consider using a more efficient approach like ExecuteUpdate
            var userProfile = await _userProfileRepository.GetByAccountIdAsync(createDto.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with account ID: {createDto.AccountId} not found");

            userProfile.TotalQuests++;
            userProfile.ExistingQuests++;

            await _userProfileRepository.UpdateAsync(userProfile, cancellationToken).ConfigureAwait(false);

            return MapToDto(createdQuest);
        }

        public async Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Type of updateDto from controller: {@updateDto}", updateDto);
            var existingQuest = await _questRepository.GetQuestByIdAsync(updateDto.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {updateDto.Id} not found");

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            existingQuest = _mapper.Map(updateDto, existingQuest);
            _logger.LogDebug("Quest updated after mapping: {@quest}", existingQuest);

            existingQuest = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (existingQuest.IsRepeatable())
            {
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);
                if (await _questOccurrenceRepository.GetCurrentOccurrenceForQuestAsync(existingQuest.Id, now, cancellationToken).ConfigureAwait(false) is null)
                {
                    var occurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesAsync(existingQuest, cancellationToken).ConfigureAwait(false);
                    _logger.LogDebug("New occurrences created: {@occurrences}", occurrences);
                }
            }

            if (questType == QuestTypeEnum.Weekly)
            {
                var weeklyUpdateDto = (UpdateWeeklyQuestDto)updateDto;
                existingQuest = _questWeekdaysHandler.HandleUpdateWeekdays(existingQuest, weeklyUpdateDto);
            }

            _logger.LogDebug("Updated quest: {@existingQuest}", existingQuest);
            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);

            var updatedQuest = await _questRepository.GetQuestByIdAsync(existingQuest.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {existingQuest.Id} not found after update.");

            return MapToDto(updatedQuest);
        }

        public async Task<BaseGetQuestDto> UpdateQuestCompletionAsync(BaseQuestCompletionPatchDto patchDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var existingQuest = await GetAndValidateQuestAsync(patchDto.Id, questType, cancellationToken);

            if (existingQuest.IsCompleted == patchDto.IsCompleted)
            {
                _logger.LogInformation("Quest {QuestId} completion status is unchanged. No update required.", existingQuest.Id);
                return MapToDto(existingQuest);
            }

            var nowUtc = SystemClock.Instance.GetCurrentInstant();

            var completionContext = await BuildCompletionContextAsync(existingQuest, patchDto, nowUtc, cancellationToken);

            await ProcessQuestCompletionAsync(existingQuest, completionContext, cancellationToken);

            existingQuest = _mapper.Map(patchDto, existingQuest);

            //await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
            await _questStatisticsService.ProcessStatisticsForQuestAsync(existingQuest, cancellationToken);

            if (completionContext.ShouldIncrementCount)
            {
                await ProcessUserRewardsAsync(existingQuest, completionContext.NowUtc, cancellationToken);
            }

            _logger.LogDebug("User profile after quest completion updated: {@existingQuest}", existingQuest.Account.Profile);
            await _userProfileRepository.UpdateAsync(existingQuest.Account.Profile, cancellationToken).ConfigureAwait(false);

            var updatedQuest = await _questRepository.GetQuestByIdAsync(existingQuest.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {existingQuest.Id} not found after completion update.");
            return MapToDto(updatedQuest);
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
            _logger.LogDebug("Today start: {TodayStart}, Today end: {TodayEnd}",
                todayStart.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
                todayEnd.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));

            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason();

            var quests = await _questRepository.GetActiveQuestsAsync(accountId, todayStart, todayEnd, currentSeason, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Quests before mapping: {@quests}", quests);

            var mappedQuests = quests.Select(MapToDto);

            _logger.LogDebug("Quests after mapping: {@mappedQuests}", mappedQuests);

            return mappedQuests;
        }

        public async Task DeleteQuestAsync(int questId, QuestTypeEnum questType, int accountId, CancellationToken cancellationToken = default)
        {
            var questToDelete = await _questRepository.GetQuestByIdAsync(questId, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (questToDelete.IsCompleted)
            {
                questToDelete.Account.Profile.CompletedExistingQuests = Math.Max(0, questToDelete.Account.Profile.CompletedExistingQuests - 1);
            }
            if (await _userGoalRepository.IsQuestActiveGoalAsync(questId, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogInformation("Deleting quest with ID: {questId} that is active goal", questId);
                questToDelete.Account.Profile.ActiveGoals = Math.Max(0, questToDelete.Account.Profile.ActiveGoals - 1);
            }

            await _questRepository.DeleteQuestAsync(questToDelete, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Deleted quest with ID: {QuestId}", questId);

            questToDelete.Account.Profile.ExistingQuests = Math.Max(0, questToDelete.Account.Profile.ExistingQuests - 1);
            await _userProfileRepository.UpdateAsync(questToDelete.Account.Profile, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetQuestEligibleForGoalAsync(int accountId, CancellationToken cancellationToken = default)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

            var quests = await _questRepository.GetQuestEligibleForGoalAsync(accountId, nowUtc, cancellationToken).ConfigureAwait(false);

            return quests.Select(MapToDto);
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

        private async Task<Quest> GetAndValidateQuestAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (quest.Account is null || string.IsNullOrWhiteSpace(quest.Account.TimeZone))
            {
                _logger.LogError("Account {AccountId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.",
                    quest.AccountId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            return quest;
        }

        private async Task<QuestCompletionContext> BuildCompletionContextAsync(
            Quest quest,
            BaseQuestCompletionPatchDto patchDto,
            Instant nowUtc,
            CancellationToken cancellationToken)
        {
            var context = new QuestCompletionContext
            {
                NowUtc = nowUtc,
                JustCompleted = !quest.IsCompleted && patchDto.IsCompleted,
                UserTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone]
                    ?? throw new NotFoundException($"Timezone with ID: {quest.Account.TimeZone} not found"),
            };

            context.Occurrence = await GetOrCreateOccurrenceAsync(quest, nowUtc, cancellationToken).ConfigureAwait(false);

            if (context.JustCompleted)
            {
                context.ShouldIncrementCount = ShouldIncrementCompletionCountAsync(quest, context);
                _logger.LogDebug("Quest {QuestId} completion context: {@context}", quest.Id, context);
            }

            return context;
        }

        // Occurrence management
        private async Task<QuestOccurrence?> GetOrCreateOccurrenceAsync(Quest quest, Instant nowUtc, CancellationToken cancellationToken)
        {
            QuestOccurrence? occurrence = null;

            if (quest.IsRepeatable())
            {
                occurrence = await _questOccurrenceRepository.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc.ToDateTimeUtc(), cancellationToken);
                _logger.LogDebug("Occurrence for quest: {@occurrence}", occurrence);

                if (occurrence is null)
                {
                    _logger.LogDebug("Missing occurrence for quest, creating new one.");
                    await _questOccurrenceGenerator.GenerateMissingOccurrencesAsync(quest, cancellationToken);
                    occurrence = await _questOccurrenceRepository.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc.ToDateTimeUtc(), cancellationToken);
                    _logger.LogDebug("Saved occurrence: {@occurrence}", occurrence);
                }
            }

            return occurrence;
        }

        // Daily completion validation
        private bool ShouldIncrementCompletionCountAsync(Quest quest, QuestCompletionContext context)
        {
            if (!quest.LastCompletedAt.HasValue)
                return true;

            var lastCompletedAtUtc = Instant.FromDateTimeUtc(DateTime.SpecifyKind(quest.LastCompletedAt.Value, DateTimeKind.Utc));
            var lastCompletedAtUserLocal = lastCompletedAtUtc.InZone(context.UserTimeZone).LocalDateTime;
            var nowUserLocal = context.NowUtc.InZone(context.UserTimeZone).LocalDateTime;

            if (lastCompletedAtUserLocal.Date < nowUserLocal.Date)
                return true;

            _logger.LogInformation("Quest {QuestId} already completed today: {CurrentTime} in user's timezone {TimeZone}. Last Completion: {LastCompletion}",
                quest.Id, nowUserLocal, quest.Account.TimeZone, lastCompletedAtUserLocal);

            return false;
        }

        // Quest completion processing
        private async Task ProcessQuestCompletionAsync(Quest quest, QuestCompletionContext context, CancellationToken cancellationToken)
        {
            if (context.JustCompleted)
            {
                await HandleQuestCompletionAsync(quest, context, cancellationToken);
            }
            else
            {
                await HandleQuestUncompletionAsync(quest, context.Occurrence, cancellationToken);
            }
        }

        private async Task HandleQuestCompletionAsync(Quest quest, QuestCompletionContext context, CancellationToken cancellationToken)
        {
            if (context.Occurrence is not null)
            {
                _logger.LogDebug("Occurrence for quest is not null, setting 'WasCompleted' to true");
                context.Occurrence.WasCompleted = true;
                context.Occurrence.CompletedAt = context.NowUtc.ToDateTimeUtc();
                //await _questOccurrenceRepository.UpdateOccurrence(context.Occurrence, cancellationToken);
            }

            quest.LastCompletedAt = context.NowUtc.ToDateTimeUtc();
            quest.NextResetAt = _questResetService.GetNextResetTimeUtc(quest);
            quest.Account.Profile.CompletedExistingQuests++;
        }

        private async Task HandleQuestUncompletionAsync(Quest quest, QuestOccurrence? occurrence, CancellationToken cancellationToken)
        {
            if (occurrence is not null)
            {
                _logger.LogDebug("Occurrence for quest is not null, setting 'WasCompleted' to false");
                occurrence.WasCompleted = false;
                //await _questOccurrenceRepository.UpdateOccurrence(occurrence, cancellationToken);
            }

            quest.Account.Profile.CompletedExistingQuests = Math.Max(0, quest.Account.Profile.CompletedExistingQuests - 1);
        }

        private async Task ProcessUserRewardsAsync(Quest quest, Instant completionTime, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileRepository.GetByAccountIdAsync(quest.AccountId, cancellationToken)
                ?? throw new NotFoundException($"User profile with account ID: {quest.AccountId} not found");

            var rewards = await _questRewardCalculator.CalculateRewardsAsync(quest, completionTime, cancellationToken);

            userProfile.CompletedQuests++;
            userProfile.TotalXp += rewards.TotalXp;

            if (rewards.GoalAchieved)
            {
                userProfile.CompletedGoals++;
                await _userGoalRepository.UpdateAsync(rewards.UserGoal!, cancellationToken);
            }

            //await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);

            _logger.LogDebug("Incremented CompletedQuests count and added {XpGained} XP for Account {AccountId}",
                rewards.TotalXp, quest.AccountId);
        }
    }
}

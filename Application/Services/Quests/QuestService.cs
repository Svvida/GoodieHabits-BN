using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces.Quests;
using Application.Models;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;
        private readonly IQuestResetService _questResetService;
        private readonly IQuestStatisticsService _questStatisticsService;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator;

        public QuestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<QuestService> logger,
            IQuestResetService questResetService,
            IQuestStatisticsService questStatisticsService,
            IQuestOccurrenceGenerator questOccurrenceGenerator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _questResetService = questResetService;
            _questStatisticsService = questStatisticsService;
            _questOccurrenceGenerator = questOccurrenceGenerator;
        }

        public async Task<BaseGetQuestDto?> GetUserQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quest = await _unitOfWork.Quests.GetQuestForDisplayAsync(questId, questType, cancellationToken).ConfigureAwait(false);

            if (quest is null)
                return null;

            _logger.LogInformation("Quest ScheduledTime: {quest.ScheduledTime}", quest.ScheduledTime);

            return MapToDto(quest);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quests = await _unitOfWork.Quests.GetQuestsByTypeForDisplayAsync(accountId, questType, cancellationToken)
                .ConfigureAwait(false);
            return quests.Select(MapToDto);
        }
        public async Task<BaseGetQuestDto> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetAccountWithProfileInfoAsync(createDto.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {createDto.AccountId} not found");

            // Check if the account is owner of the labels
            if (createDto.Labels.Count > 0)
            {
                int ownedCount = await _unitOfWork.QuestLabels.CountOwnedLabelsAsync(createDto.Labels, createDto.AccountId, cancellationToken).ConfigureAwait(false);

                if (ownedCount != createDto.Labels.Count)
                {
                    _logger.LogWarning("User {AccountId} tried to create a quest with labels they do not own.", createDto.AccountId);
                    throw new ForbiddenException("One or more provided labels do not exist or do not belong to the user.");
                }
            }

            var quest = _mapper.Map<Quest>(createDto);
            _logger.LogDebug("Quest created after mapping: {@quest}", quest);

            quest.Account = account;

            if (quest.IsRepeatable())
            {
                quest.Statistics = new QuestStatistics();
                quest.NextResetAt = _questResetService.GetNextResetTimeUtc(quest);
                quest.QuestOccurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken).ConfigureAwait(false);
            }

            await _unitOfWork.Quests.AddAsync(quest, cancellationToken);

            quest.Account.Profile.TotalQuests++;
            quest.Account.Profile.ExistingQuests++;

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MapToDto(quest);
        }

        public async Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            // Check if the account is owner of the labels
            if (updateDto.Labels.Count > 0)
            {
                int ownedCount = await _unitOfWork.QuestLabels.CountOwnedLabelsAsync(updateDto.Labels, updateDto.AccountId, cancellationToken).ConfigureAwait(false);

                if (ownedCount != updateDto.Labels.Count)
                {
                    _logger.LogWarning("User {AccountId} tried to create a quest with labels they do not own.", updateDto.AccountId);
                    throw new ForbiddenException("One or more provided labels do not exist or do not belong to the user.");
                }
            }

            var existingQuest = await _unitOfWork.Quests.GetQuestByIdAsync(updateDto.Id, questType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {updateDto.Id} not found");

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            existingQuest = _mapper.Map(updateDto, existingQuest);

            // Update labels if provided
            var existingLabels = existingQuest.Quest_QuestLabels.ToList();

            HashSet<int> existingLabelsHashSet = [.. existingQuest.Quest_QuestLabels.Select(x => x.QuestLabelId)];
            HashSet<int> newLabelsHashSet = [.. updateDto.Labels];

            _logger.LogDebug("Existing labels: {@existingLabels}", existingLabels);
            _logger.LogDebug("New labels: {@newLabels}", newLabelsHashSet);

            var labelsToAdd = updateDto.Labels
                .Where(labelId => !existingLabelsHashSet.Contains(labelId))
                .Select(labelId => new Quest_QuestLabel
                {
                    QuestId = existingQuest.Id,
                    QuestLabelId = labelId
                }).ToList();

            var labelsToRemove = existingLabels
                .Where(existingLabel => !newLabelsHashSet.Contains(existingLabel.QuestLabelId))
                .ToList();

            foreach (var label in labelsToAdd)
            {
                existingQuest.Quest_QuestLabels.Add(label);
            }

            foreach (var label in labelsToRemove)
            {
                existingQuest.Quest_QuestLabels.Remove(label);
            }

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (existingQuest.IsRepeatable())
            {
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);
                if (await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(existingQuest.Id, now, cancellationToken).ConfigureAwait(false) is null)
                {
                    var generatedOccurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(existingQuest, cancellationToken).ConfigureAwait(false);
                    if (generatedOccurrences.Count != 0)
                    {
                        foreach (var generatedOccurrence in generatedOccurrences)
                        {
                            _logger.LogDebug("Adding generated occurrence: {@generatedOccurrence}", generatedOccurrence);
                            existingQuest.QuestOccurrences.Add(generatedOccurrence);
                        }
                    }
                }
                existingQuest.Statistics = await _unitOfWork.QuestStatistics.GetStatisticsForQuestAsync(existingQuest.Id, true, cancellationToken).ConfigureAwait(false);
            }

            if (questType == QuestTypeEnum.Weekly)
            {
                var weeklyUpdateDto = (UpdateWeeklyQuestDto)updateDto;

                var desiredWeekdays = weeklyUpdateDto.Weekdays.Select(wd => Enum.Parse<WeekdayEnum>(wd)).ToHashSet();
                var existingWeekdays = existingQuest.WeeklyQuest_Days.Select(wqd => wqd.Weekday).ToHashSet();

                var weekdaysToAdd = desiredWeekdays.Except(existingWeekdays);
                foreach (var weekday in weekdaysToAdd)
                {
                    existingQuest.WeeklyQuest_Days.Add(new WeeklyQuest_Day
                    {
                        Weekday = weekday
                    });
                }

                var weekdaysToRemove = existingQuest.WeeklyQuest_Days
                    .Where(wqd => !desiredWeekdays.Contains(wqd.Weekday))
                    .ToList();
                foreach (var weekday in weekdaysToRemove)
                {
                    existingQuest.WeeklyQuest_Days.Remove(weekday);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MapToDto(existingQuest);

        }

        public async Task UpdateQuestCompletionAsync(QuestCompletionPatchDto patchDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var existingQuest = await GetAndValidateQuestAsync(patchDto.Id, questType, cancellationToken);

            if (existingQuest.IsCompleted == patchDto.IsCompleted)
            {
                _logger.LogInformation("Quest {QuestId} completion status is unchanged. No update required.", existingQuest.Id);
                return;
            }

            var nowUtc = SystemClock.Instance.GetCurrentInstant();

            var completionContext = await BuildCompletionContextAsync(existingQuest, patchDto, nowUtc, cancellationToken);

            ProcessQuestCompletion(existingQuest, completionContext);

            existingQuest = _mapper.Map(patchDto, existingQuest);

            if (completionContext.ShouldIncrementCount)
            {
                ProcessUserRewards(existingQuest, completionContext.NowUtc);
            }

            //_logger.LogDebug("User profile after quest completion updated: {@existingQuest}", existingQuest.Account.Profile);

            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Quest {QuestId} completion updated with {AffectedRows} affected rows.", existingQuest.Id, affectedRows);

            if (existingQuest.IsRepeatable())
                await _questStatisticsService.ProcessStatisticsForQuestAndSaveAsync(existingQuest.Id, cancellationToken).ConfigureAwait(false);

            return;
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(
            int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
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

            var quests = await _unitOfWork.Quests.GetActiveQuestsForDisplayAsync(accountId, todayStart, todayEnd, currentSeason, cancellationToken)
                .ConfigureAwait(false);

            return quests.Select(MapToDto);
        }

        public async Task DeleteQuestAsync(int questId, QuestTypeEnum questType, int accountId, CancellationToken cancellationToken = default)
        {
            var questToDelete = await _unitOfWork.Quests.GetByIdAsync(questId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            var account = await _unitOfWork.Accounts.GetAccountWithProfileInfoAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            questToDelete.Account = account;

            if (questToDelete.IsCompleted)
                questToDelete.Account.Profile.CurrentlyCompletedExistingQuests = Math.Max(0, questToDelete.Account.Profile.CurrentlyCompletedExistingQuests - 1);

            if (questToDelete.WasEverCompleted)
                questToDelete.Account.Profile.EverCompletedExistingQuests = Math.Max(0, questToDelete.Account.Profile.EverCompletedExistingQuests - 1);

            if (await _unitOfWork.UserGoals.IsQuestActiveGoalAsync(questId, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogInformation("Deleting quest with ID: {questId} that is active goal", questId);
                questToDelete.Account.Profile.ActiveGoals = Math.Max(0, questToDelete.Account.Profile.ActiveGoals - 1);
            }

            _unitOfWork.Quests.Delete(questToDelete);
            _logger.LogInformation("Deleted quest with ID: {QuestId}", questId);

            questToDelete.Account.Profile.ExistingQuests = Math.Max(0, questToDelete.Account.Profile.ExistingQuests - 1);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetQuestEligibleForGoalAsync(int accountId, CancellationToken cancellationToken = default)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

            var quests = await _unitOfWork.Quests.GetQuestEligibleForGoalAsync(accountId, nowUtc, cancellationToken).ConfigureAwait(false);

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
            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(questId, questType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (quest.Account is null || string.IsNullOrWhiteSpace(quest.Account.TimeZone))
            {
                _logger.LogError("Account {AccountId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.",
                    quest.AccountId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            var goal = await _unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(questId, cancellationToken).ConfigureAwait(false);
            if (goal is not null)
                quest.UserGoal.Add(goal);

            return quest;
        }

        private async Task<QuestCompletionContext> BuildCompletionContextAsync(
            Quest quest,
            QuestCompletionPatchDto patchDto,
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
                //_logger.LogDebug("Quest {QuestId} completion context: {@context}", quest.Id, context);
            }

            return context;
        }

        // Occurrence management
        private async Task<QuestOccurrence?> GetOrCreateOccurrenceAsync(Quest quest, Instant nowUtc, CancellationToken cancellationToken)
        {
            QuestOccurrence? occurrence = null;

            if (quest.IsRepeatable())
            {
                occurrence = await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc.ToDateTimeUtc(), cancellationToken);
                //_logger.LogDebug("Occurrence for quest: {@occurrence}", occurrence);

                if (occurrence is null)
                {
                    _logger.LogDebug("Missing occurrence for quest, creating new one.");
                    var generatedMissingOccurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken);
                    var sortedOccurrences = generatedMissingOccurrences.OrderByDescending(o => o.OccurrenceEnd).ToList();
                    occurrence = sortedOccurrences.FirstOrDefault(o => o.OccurrenceStart <= nowUtc.ToDateTimeUtc() && o.OccurrenceEnd >= nowUtc.ToDateTimeUtc());
                    //occurrence = await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc.ToDateTimeUtc(), cancellationToken);
                    //_logger.LogDebug("Current occurrence: {@occurrence}", occurrence);
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
        private void ProcessQuestCompletion(Quest quest, QuestCompletionContext context)
        {
            if (context.JustCompleted)
            {
                HandleQuestCompletion(quest, context);
            }
            else
            {
                HandleQuestUncompletion(quest, context);
            }
        }

        private void HandleQuestCompletion(Quest quest, QuestCompletionContext context)
        {
            if (context.Occurrence is not null)
            {
                _logger.LogDebug("Occurrence for quest is not null, setting 'WasCompleted' to true");
                context.Occurrence.WasCompleted = true;
                context.Occurrence.CompletedAt = context.NowUtc.ToDateTimeUtc();
            }

            if (quest.WasEverCompleted is false)
            {
                quest.WasEverCompleted = true;
                quest.Account.Profile.EverCompletedExistingQuests++;
            }

            quest.LastCompletedAt = context.NowUtc.ToDateTimeUtc();
            quest.NextResetAt = _questResetService.GetNextResetTimeUtc(quest);
            quest.Account.Profile.CurrentlyCompletedExistingQuests++;

            if (quest.UserGoal is not null)
            {
                foreach (var goal in quest.UserGoal)
                {
                    goal.IsAchieved = true;
                    goal.AchievedAt = context.NowUtc.ToDateTimeUtc();
                    quest.Account.Profile.CompletedGoals++;
                }
            }
        }

        private void HandleQuestUncompletion(Quest quest, QuestCompletionContext context)
        {
            if (context.Occurrence is not null)
            {
                _logger.LogDebug("Occurrence for quest is not null, setting 'WasCompleted' to false");
                context.Occurrence.WasCompleted = false;
            }

            quest.Account.Profile.CurrentlyCompletedExistingQuests = Math.Max(0, quest.Account.Profile.CurrentlyCompletedExistingQuests - 1);

            if (quest.UserGoal is not null)
            {
                foreach (var goal in quest.UserGoal)
                {
                    goal.IsAchieved = false;
                    goal.AchievedAt = context.NowUtc.ToDateTimeUtc();
                    quest.Account.Profile.CompletedGoals = Math.Max(quest.Account.Profile.CompletedGoals - 1, 0);
                }
            }
        }

        private void ProcessUserRewards(Quest quest, Instant completionTime)
        {
            var rewards = CalculateRewards(quest, completionTime);

            quest.Account.Profile.CompletedQuests++;
            quest.Account.Profile.TotalXp += rewards.TotalXp;

            _logger.LogDebug("Incremented CompletedQuests count and added {XpGained} XP for Account {AccountId}",
                rewards.TotalXp, quest.AccountId);
        }

        private QuestRewards CalculateRewards(Quest quest, Instant completionTime)
        {
            var rewards = new QuestRewards();

            foreach (var userGoal in quest.UserGoal)
            {
                rewards.GoalAchieved = true;
                rewards.UserGoal = userGoal;
                rewards.TotalXp += userGoal.XpBonus;

                _logger.LogInformation("User achieved goal ID {GoalId} and earned bonus {XpBonus} XP", userGoal.Id, userGoal.XpBonus);
            }

            return rewards;
        }
    }
}

using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Dtos.UserGoal;
using Application.Interfaces;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services
{
    public class UserGoalService : IUserGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserGoalService> _logger;

        public UserGoalService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserGoalService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task CreateUserGoalAsync(CreateUserGoalDto goalDto, CancellationToken cancellationToken = default)
        {
            QuestTypeEnum questType = (QuestTypeEnum)Enum.Parse(typeof(QuestTypeEnum), goalDto.QuestType, true);
            GoalTypeEnum goalType = (GoalTypeEnum)Enum.Parse(typeof(GoalTypeEnum), goalDto.GoalType, true);

            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(goalDto.QuestId, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {goalDto.QuestId} of type {questType} not found.");

            if (quest.IsCompleted == true)
                throw new ConflictException($"Quest with ID {goalDto.QuestId} is already completed. Cannot create a goal for it.");

            if (!await CanCreateGoalAsync(quest.AccountId, quest.Id, goalType, cancellationToken))
                throw new ConflictException($"Cannot create goal of type {goalType} for quest with ID {goalDto.QuestId}. This quest is already other type of goal or numbers of current goals is exceeded.");

            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];
            DateTime endsAtUtc = CalculateGoalEndTime(goalType, userTimeZone);

            int bonusXp;
            switch (goalType)
            {
                case GoalTypeEnum.Daily:
                    bonusXp = 5;
                    break;
                case GoalTypeEnum.Weekly:
                    bonusXp = 10;
                    break;
                case GoalTypeEnum.Monthly:
                    bonusXp = 20;
                    break;
                case GoalTypeEnum.Yearly:
                    bonusXp = 50;
                    break;
                default:
                    throw new InvalidArgumentException($"Invalid goal type: {goalType}.");
            }

            var userGoal = new UserGoal
            {
                QuestId = goalDto.QuestId,
                AccountId = quest.AccountId,
                GoalType = goalType,
                CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
                EndsAt = endsAtUtc,
                IsAchieved = false,
                IsExpired = false,
                XpBonus = bonusXp,
            };

            await _unitOfWork.UserGoals.AddAsync(userGoal, cancellationToken).ConfigureAwait(false);

            var userProfile = await _unitOfWork.UserProfiles.GetByAccountIdAsync(quest.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile for account ID {quest.AccountId} not found.");

            userProfile.TotalGoals++;
            userProfile.ActiveGoals++;

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("User profile for account ID {quest.AccountId} updated. Updated profile goals: {@userProfile}.", quest.AccountId, userProfile);
        }

        public async Task<BaseGetQuestDto?> GetUserActiveGoalByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default)
        {
            var userGoal = await _unitOfWork.UserGoals.GetUserActiveGoalByTypeAsync(accountId, goalType, cancellationToken).ConfigureAwait(false);
            if (userGoal == null)
                return null;
            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(userGoal.QuestId, userGoal.Quest.QuestType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {userGoal.QuestId} not found");
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

        private static DateTime CalculateGoalEndTime(GoalTypeEnum goalType, DateTimeZone userTimeZone)
        {
            Instant nowUtc = SystemClock.Instance.GetCurrentInstant();
            LocalDateTime nowLocal = nowUtc.InZone(userTimeZone).LocalDateTime;

            LocalDateTime endLocal;

            switch (goalType)
            {
                case GoalTypeEnum.Daily:
                    endLocal = nowLocal.Date.PlusDays(1).AtMidnight();
                    break;
                case GoalTypeEnum.Weekly:
                    var daysUntilMonday = (IsoDayOfWeek.Monday - nowLocal.DayOfWeek + 7) % 7;
                    daysUntilMonday = daysUntilMonday == 0 ? 7 : daysUntilMonday; // If today is Monday, set to next Monday
                    endLocal = nowLocal.Date.PlusDays(daysUntilMonday).AtMidnight();
                    break;
                case GoalTypeEnum.Monthly:
                    endLocal = new LocalDateTime(nowLocal.Year, nowLocal.Month, 1, 0, 0, 0)
                        .PlusMonths(1);
                    break;
                case GoalTypeEnum.Yearly:
                    endLocal = new LocalDateTime(nowLocal.Year, 1, 1, 0, 0, 0)
                        .PlusYears(1);
                    break;
                default:
                    throw new InvalidArgumentException($"Invalid goal type: {goalType}.");
            }

            return endLocal.InZoneLeniently(userTimeZone).ToDateTimeUtc();
        }

        private async Task<bool> CanCreateGoalAsync(int accountId, int questId, GoalTypeEnum goalType, CancellationToken cancellationToken = default)
        {
            // Check if quest is already an active goal
            if (await _unitOfWork.UserGoals.IsQuestActiveGoalAsync(questId, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogDebug($"Quest with ID {questId} is already an active goal.");
                return false;
            }

            // Check if there are already active goals of the same type
            var activeGoalsCount = await _unitOfWork.UserGoals.GetActiveGoalsCountByTypeAsync(accountId, goalType, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug($"Number of user goals: {activeGoalsCount} of type {goalType} for user {accountId}.");

            return goalType switch
            {
                GoalTypeEnum.Daily => activeGoalsCount < 1,
                GoalTypeEnum.Weekly => activeGoalsCount < 1,
                GoalTypeEnum.Monthly => activeGoalsCount < 1,
                GoalTypeEnum.Yearly => activeGoalsCount < 1,
                _ => false,
            };
        }
    }
}

using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.Quests
{
    public class QuestRepository : IQuestRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestRepository> _logger;

        public QuestRepository(
            AppDbContext context,
            ILogger<QuestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Quest>> GetActiveQuestsAsync(
            int accountId,
            DateTime todayStart,
            DateTime todayEnd,
            SeasonEnum currentSeason,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _context.Quests
                .Where(q => q.AccountId == accountId)
                .Where(q =>
                    (q.QuestType == QuestTypeEnum.OneTime &&
                        (q.StartDate ?? DateTime.MinValue) <= todayStart &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayEnd &&
                        (q.StartDate.HasValue || q.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Daily &&
                        (q.StartDate ?? DateTime.MinValue) <= todayStart &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayEnd)

                    || (q.QuestType == QuestTypeEnum.Weekly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayStart &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayEnd &&
                        (q.WeeklyQuest_Days.Any(wd => wd.Weekday == (WeekdayEnum)todayStart.DayOfWeek)))

                    || (q.QuestType == QuestTypeEnum.Monthly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayStart &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayEnd &&
                        (q.MonthlyQuest_Days!.StartDay <= todayStart.Day && q.MonthlyQuest_Days.EndDay >= todayEnd.Day))

                    || (q.QuestType == QuestTypeEnum.Seasonal &&
                        (q.StartDate ?? DateTime.MinValue) <= todayStart &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayEnd &&
                        (q.SeasonalQuest_Season!.Season == currentSeason))
                )
                .Include(q => q.Quest_QuestLabels)
                .AsNoTracking()
                .AsQueryable();

            var result = await ApplyQuestProjection(baseQuery).ToListAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Fetched {@result} quests from repository.", result);
            return result;
        }

        public async Task<IEnumerable<Quest>> GetQuestsByTypeAsync(
            int accountId,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var quests = _context.Quests
                .Where(q => q.AccountId == accountId && q.QuestType == questType)
                .Include(q => q.Quest_QuestLabels)
                .ThenInclude(ql => ql.QuestLabel)
                .AsNoTracking();

            if (questType == QuestTypeEnum.Monthly)
            {
                quests = quests.Include(q => q.MonthlyQuest_Days).AsNoTracking();
            }
            if (questType == QuestTypeEnum.Weekly)
            {
                quests = quests.Include(q => q.WeeklyQuest_Days).AsNoTracking();
            }
            if (questType == QuestTypeEnum.Seasonal)
            {
                quests = quests.Include(q => q.SeasonalQuest_Season).AsNoTracking();
            }

            var result = await ApplyQuestProjection(quests).ToListAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Fetched {@result} quests from repository.", result);
            return result;
        }

        public async Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quest = _context.Quests
                .Where(q => q.Id == questId && q.QuestType == questType)
                .Include(q => q.Quest_QuestLabels)
                .ThenInclude(ql => ql.QuestLabel)
                .AsNoTracking();

            if (questType == QuestTypeEnum.Monthly)
            {
                quest = quest.Include(q => q.MonthlyQuest_Days).AsNoTracking();
            }
            if (questType == QuestTypeEnum.Weekly)
            {
                quest = quest.Include(q => q.WeeklyQuest_Days).AsNoTracking();
            }
            if (questType == QuestTypeEnum.Seasonal)
            {
                quest = quest.Include(q => q.SeasonalQuest_Season).AsNoTracking();
            }

            var result = await ApplyQuestProjection(quest).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Fetched {@result} quests from repository.", result);
            return result;
        }

        public async Task DeleteQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var result = await _context.Quests.Where(q => q.Id == questId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result == 0)
            {
                // This should never happen if AuthorizationFilter is working correctly. Keeping it here anyways for safety.
                _logger.LogWarning("Quest with id {questId} not found.", questId);
                throw new InvalidArgumentException($"Failed to delete quest with ID {questId}.  Possible authorization failure or data inconsistency.");
            }
        }

        public void AddQuestLabels(List<Quest_QuestLabel> labelsToAdd)
        {
            _context.Quest_QuestLabels.AddRange(labelsToAdd);
        }

        public void RemoveQuestLabels(List<Quest_QuestLabel> labelsToRemove)
        {
            _context.Quest_QuestLabels.RemoveRange(labelsToRemove);
        }

        public async Task<bool> IsQuestOwnedByUserAsync(
            int questId,
            int accountId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .AnyAsync(q => q.Id == questId &&
                        q.AccountId == accountId,
                        cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task AddQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            _context.Quests.Add(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            _context.Quests.Update(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public void AddQuestWeekdays(List<WeeklyQuest_Day> weekdaysToAdd)
        {
            _context.WeeklyQuest_Days.AddRange(weekdaysToAdd);
        }
        public void RemoveQuestWeekdays(List<WeeklyQuest_Day> weekdaysToRemove)
        {
            _context.WeeklyQuest_Days.RemoveRange(weekdaysToRemove);
        }

        private static IQueryable<Quest> ApplyQuestProjection(IQueryable<Quest> query)
        {
            return query.Select(q => new Quest
            {
                Id = q.Id,
                AccountId = q.AccountId,
                Account = new Account
                {
                    Id = q.Account.Id,
                    Login = q.Account.Login,
                    HashPassword = q.Account.HashPassword,
                    Email = q.Account.Email,
                    TimeZone = q.Account.TimeZone,
                    CreatedAt = q.Account.CreatedAt,
                    UpdatedAt = q.Account.UpdatedAt,
                    Profile = new UserProfile
                    {
                        Id = q.Account.Profile.Id,
                        AccountId = q.AccountId,
                        TotalQuests = q.Account.Profile.TotalQuests,
                        CompletedQuests = q.Account.Profile.CompletedQuests,
                        TotalExperience = q.Account.Profile.TotalExperience,
                        CurrentExperience = q.Account.Profile.CurrentExperience,
                        CreatedAt = q.Account.Profile.CreatedAt,
                        UpdatedAt = q.Account.Profile.UpdatedAt,
                        UserProfile_Badges = q.Account.Profile.UserProfile_Badges.Select(upb => new UserProfile_Badge
                        {
                            BadgeId = upb.BadgeId,
                            UserProfileId = upb.UserProfileId,
                            EarnedAt = upb.EarnedAt,
                            Badge = new Badge
                            {
                                Id = upb.Badge.Id,
                                Text = upb.Badge.Text
                            }
                        }).ToList()
                    }
                },
                QuestType = q.QuestType,
                Title = q.Title,
                Description = q.Description,
                Priority = q.Priority,
                IsCompleted = q.IsCompleted,
                Emoji = q.Emoji,
                StartDate = q.StartDate,
                EndDate = q.EndDate,
                LastCompletedAt = q.LastCompletedAt,
                NextResetAt = q.NextResetAt,
                UpdatedAt = q.UpdatedAt,
                CreatedAt = q.CreatedAt,

                Quest_QuestLabels = q.Quest_QuestLabels.Select(ql => new Quest_QuestLabel
                {
                    QuestId = q.Id,
                    QuestLabelId = ql.QuestLabelId,
                    QuestLabel = new QuestLabel
                    {
                        Id = ql.QuestLabelId,
                        AccountId = q.AccountId,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor,
                        UpdatedAt = ql.QuestLabel.UpdatedAt,
                        CreatedAt = ql.QuestLabel.CreatedAt
                    }
                }).ToList(),

                SeasonalQuest_Season = q.SeasonalQuest_Season != null
                    ? new SeasonalQuest_Season
                    {
                        Id = q.SeasonalQuest_Season.Id,
                        QuestId = q.Id,
                        Season = q.SeasonalQuest_Season.Season
                    }
                    : null,

                MonthlyQuest_Days = q.MonthlyQuest_Days != null
                    ? new MonthlyQuest_Days
                    {
                        Id = q.MonthlyQuest_Days.Id,
                        QuestId = q.Id,
                        StartDay = q.MonthlyQuest_Days.StartDay,
                        EndDay = q.MonthlyQuest_Days.EndDay
                    }
                    : null,

                WeeklyQuest_Days = q.WeeklyQuest_Days != null
                    ? q.WeeklyQuest_Days.Select(wd => new WeeklyQuest_Day
                    {
                        Id = wd.Id,
                        QuestId = q.Id,
                        Weekday = wd.Weekday
                    }).ToList()
                    : new List<WeeklyQuest_Day>()
            }).AsNoTracking();
        }
    }
}

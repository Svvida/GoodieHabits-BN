using Domain.Enum;
using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestRepository : BaseRepository<Quest>, IQuestRepository
    {
        public QuestRepository(AppDbContext context) : base(context) { }

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
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        (q.StartDate.HasValue || q.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Daily &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart)

                    || (q.QuestType == QuestTypeEnum.Weekly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        q.WeeklyQuest_Days.Any(wd =>
                            wd.Weekday == (WeekdayEnum)todayStart.DayOfWeek ||
                            wd.Weekday == (WeekdayEnum)todayEnd.DayOfWeek))

                    || (q.QuestType == QuestTypeEnum.Monthly &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        ((q.MonthlyQuest_Days!.StartDay <= todayStart.Day && q.MonthlyQuest_Days.EndDay >= todayStart.Day)
                        ||
                        (q.MonthlyQuest_Days.StartDay <= todayEnd.Day && q.MonthlyQuest_Days.EndDay >= todayEnd.Day)))

                    || (q.QuestType == QuestTypeEnum.Seasonal &&
                        (q.StartDate ?? DateTime.MinValue) <= todayEnd &&
                        (q.EndDate ?? DateTime.MaxValue) >= todayStart &&
                        (q.SeasonalQuest_Season!.Season == currentSeason))
                )
                .AsNoTracking()
                .Include(q => q.Quest_QuestLabels)
                .AsNoTracking()
                .AsQueryable();

            var projectedQuery = baseQuery.Select(q => new Quest
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

                Statistics = q.Statistics != null
                    ? new QuestStatistics
                    {
                        Id = q.Statistics.Id,
                        QuestId = q.Id,
                        OccurrenceCount = q.Statistics.OccurrenceCount,
                        CompletionCount = q.Statistics.CompletionCount,
                        FailureCount = q.Statistics.FailureCount,
                        CurrentStreak = q.Statistics.CurrentStreak,
                        LongestStreak = q.Statistics.LongestStreak,
                        LastCompletedAt = q.Statistics.LastCompletedAt
                    }
                    : null,

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

            return await projectedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
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

            return await ApplyQuestProjection(quests).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quest = _context.Quests
                .Where(q => q.Id == questId && q.QuestType == questType)
                .AsNoTracking()
                .Include(q => q.Account)
                    .ThenInclude(a => a.Profile)
                .AsNoTracking()
                .Include(q => q.Statistics)
                .AsNoTracking()
                .Include(q => q.QuestOccurrences)
                .AsNoTracking()
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

            return await ApplyQuestProjection(quest).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Quest>> GetRepeatableQuestsAsync(CancellationToken cancellationToken = default)
        {
            var quests = _context.Quests
                .Include(q => q.Account)
                .AsNoTracking()
                .Include(q => q.Statistics)
                .Include(q => q.QuestOccurrences)
                .Include(q => q.WeeklyQuest_Days)
                .AsNoTracking()
                .Include(q => q.MonthlyQuest_Days)
                .AsNoTracking()
                .Where(q => q.QuestType == QuestTypeEnum.Daily ||
                q.QuestType == QuestTypeEnum.Weekly ||
                q.QuestType == QuestTypeEnum.Monthly)
                .AsNoTracking();

            var projectedQuests = quests.Select(q => new Quest
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

                Statistics = q.Statistics != null
                    ? new QuestStatistics
                    {
                        Id = q.Statistics.Id,
                        QuestId = q.Id,
                        OccurrenceCount = q.Statistics.OccurrenceCount,
                        CompletionCount = q.Statistics.CompletionCount,
                        FailureCount = q.Statistics.FailureCount,
                        CurrentStreak = q.Statistics.CurrentStreak,
                        LongestStreak = q.Statistics.LongestStreak,
                        LastCompletedAt = q.Statistics.LastCompletedAt
                    }
                    : null,

                QuestOccurrences = q.QuestOccurrences != null ? q.QuestOccurrences.Select(qo => new QuestOccurrence
                {
                    Id = qo.Id,
                    QuestId = qo.QuestId,
                    OccurrenceStart = qo.OccurrenceStart,
                    OccurrenceEnd = qo.OccurrenceEnd,
                    WasCompleted = qo.WasCompleted,
                }).ToList()
                : new List<QuestOccurrence>(),

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

            return await projectedQuests.ToListAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int accountId, DateTime now, CancellationToken cancellationToken = default)
        {
            var activeUserGoalsIds = await _context.UserGoals
                .Where(g => g.AccountId == accountId && !g.IsExpired)
                .AsNoTracking()
                .Select(g => g.QuestId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return await _context.Quests
                .Where(q => q.AccountId == accountId &&
                            !q.IsCompleted &&
                            (q.EndDate ?? DateTime.MaxValue) > now &&
                            !activeUserGoalsIds.Contains(q.Id))
                .AsNoTracking()
                .Include(q => q.WeeklyQuest_Days)
                .Include(q => q.MonthlyQuest_Days)
                .Include(q => q.SeasonalQuest_Season)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<IEnumerable<Quest>> GetQuestsEligibleForResetAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Quests
                .Where(q => q.IsCompleted &&
                       (q.EndDate ?? DateTime.MaxValue) >= DateTime.UtcNow &&
                       (q.NextResetAt.HasValue && q.NextResetAt <= DateTime.UtcNow))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
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
                        ExistingQuests = q.Account.Profile.ExistingQuests,
                        CompletedExistingQuests = q.Account.Profile.CompletedExistingQuests,
                        CompletedGoals = q.Account.Profile.CompletedGoals,
                        ExpiredGoals = q.Account.Profile.ExpiredGoals,
                        TotalGoals = q.Account.Profile.TotalGoals,
                        ActiveGoals = q.Account.Profile.ActiveGoals,
                        TotalXp = q.Account.Profile.TotalXp,
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

                Statistics = q.Statistics != null
                    ? new QuestStatistics
                    {
                        Id = q.Statistics.Id,
                        QuestId = q.Id,
                        OccurrenceCount = q.Statistics.OccurrenceCount,
                        CompletionCount = q.Statistics.CompletionCount,
                        FailureCount = q.Statistics.FailureCount,
                        CurrentStreak = q.Statistics.CurrentStreak,
                        LongestStreak = q.Statistics.LongestStreak,
                        LastCompletedAt = q.Statistics.LastCompletedAt
                    }
                    : null,

                QuestOccurrences = q.QuestOccurrences != null ? q.QuestOccurrences.Select(qo => new QuestOccurrence
                {
                    Id = qo.Id,
                    QuestId = qo.QuestId,
                    OccurrenceStart = qo.OccurrenceStart,
                    OccurrenceEnd = qo.OccurrenceEnd,
                    WasCompleted = qo.WasCompleted,
                }).ToList()
                : new List<QuestOccurrence>(),

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

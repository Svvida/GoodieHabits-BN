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

        public async Task<IEnumerable<Quest>> GetActiveQuestsAsync(int accountId, SeasonEnum currentSeason, CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.UtcNow.Date;

            var baseQuery = _context.Quests
                .Where(q => q.AccountId == accountId)
                .Where(q =>
                    (q.QuestType == QuestTypeEnum.OneTime &&
                        (q.StartDate ?? DateTime.MinValue) <= today &&
                        (q.EndDate ?? DateTime.MaxValue) >= today &&
                        (q.StartDate.HasValue || q.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Daily &&
                        (q.StartDate ?? DateTime.MinValue) <= today &&
                        (q.EndDate ?? DateTime.MaxValue) >= today)

                    || (q.QuestType == QuestTypeEnum.Weekly &&
                        (q.StartDate ?? DateTime.MinValue) <= today &&
                        (q.EndDate ?? DateTime.MaxValue) >= today &&
                        (q.WeeklyQuest_Days.Any(wd => wd.Weekday == (WeekdayEnum)today.DayOfWeek)))

                    || (q.QuestType == QuestTypeEnum.Monthly &&
                        (q.StartDate ?? DateTime.MinValue) <= today &&
                        (q.EndDate ?? DateTime.MaxValue) >= today &&
                        (q.MonthlyQuest_Days!.StartDay <= today.Day && q.MonthlyQuest_Days.EndDay >= today.Day))

                    || (q.QuestType == QuestTypeEnum.Seasonal &&
                        (q.StartDate ?? DateTime.MinValue) <= today &&
                        (q.EndDate ?? DateTime.MaxValue) >= today &&
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

        public async Task AddQuestLabelsAsync(List<Quest_QuestLabel> labelsToAdd, CancellationToken cancellationToken = default)
        {
            _context.Quest_QuestLabels.AddRange(labelsToAdd);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveQuestLabelsAsync(List<Quest_QuestLabel> labelsToRemove, CancellationToken cancellationToken = default)
        {
            _context.Quest_QuestLabels.RemoveRange(labelsToRemove);
            await _context.SaveChangesAsync(cancellationToken);
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

        private static IQueryable<Quest> ApplyQuestProjection(IQueryable<Quest> query)
        {
            return query.Select(q => new Quest
            {
                Id = q.Id,
                QuestType = q.QuestType,
                Title = q.Title,
                Description = q.Description,
                Priority = q.Priority,
                IsCompleted = q.IsCompleted,
                Emoji = q.Emoji,
                StartDate = q.StartDate,
                EndDate = q.EndDate,

                Quest_QuestLabels = q.Quest_QuestLabels.Select(ql => new Quest_QuestLabel
                {
                    QuestLabel = new QuestLabel
                    {
                        Id = ql.QuestLabelId,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }
                }).ToList(),

                SeasonalQuest_Season = q.SeasonalQuest_Season != null
                    ? new SeasonalQuest_Season
                    {
                        Season = q.SeasonalQuest_Season.Season
                    }
                    : null,

                MonthlyQuest_Days = q.MonthlyQuest_Days != null
                    ? new MonthlyQuest_Days
                    {
                        StartDay = q.MonthlyQuest_Days.StartDay,
                        EndDay = q.MonthlyQuest_Days.EndDay
                    }
                    : null,

                WeeklyQuest_Days = q.WeeklyQuest_Days != null
                    ? q.WeeklyQuest_Days.Select(wd => new WeeklyQuest_Day
                    {
                        Weekday = wd.Weekday
                    }).ToList()
                    : new List<WeeklyQuest_Day>()
            }).AsNoTracking();
        }
    }
}

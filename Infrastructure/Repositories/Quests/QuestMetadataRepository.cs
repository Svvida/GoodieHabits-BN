using Domain.Enum;
using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.Quests
{
    public class QuestMetadataRepository : IQuestMetadataRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestMetadataRepository> _logger;

        public QuestMetadataRepository(
            AppDbContext context,
            ILogger<QuestMetadataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<QuestMetadata>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.UtcNow.Date;

            var baseQuery = _context.QuestsMetadata
                .Where(q => q.AccountId == accountId)
                .Where(q =>
                    (q.QuestType == QuestTypeEnum.OneTime && q.OneTimeQuest != null &&
                     (!q.OneTimeQuest.StartDate.HasValue || q.OneTimeQuest.StartDate.Value.Date <= today) &&
                     (!q.OneTimeQuest.EndDate.HasValue || q.OneTimeQuest.EndDate.Value.Date >= today) &&
                     (q.OneTimeQuest.StartDate.HasValue || q.OneTimeQuest.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Daily && q.DailyQuest != null &&
                        (!q.DailyQuest.StartDate.HasValue || q.DailyQuest.StartDate.Value.Date <= today) &&
                        (!q.DailyQuest.EndDate.HasValue || q.DailyQuest.EndDate.Value.Date >= today))

                    || (q.QuestType == QuestTypeEnum.Weekly && q.WeeklyQuest != null &&
                        (!q.WeeklyQuest.StartDate.HasValue || q.WeeklyQuest.StartDate.Value.Date <= today) &&
                        (!q.WeeklyQuest.EndDate.HasValue || q.WeeklyQuest.EndDate.Value.Date >= today) &&
                        (q.WeeklyQuest.StartDate.HasValue || q.WeeklyQuest.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Monthly && q.MonthlyQuest != null &&
                        (!q.MonthlyQuest.StartDate.HasValue || q.MonthlyQuest.StartDate.Value.Date <= today) &&
                        (!q.MonthlyQuest.EndDate.HasValue || q.MonthlyQuest.EndDate.Value.Date >= today) &&
                        (q.MonthlyQuest.StartDate.HasValue || q.MonthlyQuest.EndDate.HasValue))

                    || (q.QuestType == QuestTypeEnum.Seasonal && q.SeasonalQuest != null &&
                        (!q.SeasonalQuest.StartDate.HasValue || q.SeasonalQuest.StartDate.Value.Date <= today) &&
                        (!q.SeasonalQuest.EndDate.HasValue || q.SeasonalQuest.EndDate.Value.Date >= today) &&
                        (q.SeasonalQuest.StartDate.HasValue || q.SeasonalQuest.EndDate.HasValue))
                )
                .AsQueryable();

            baseQuery = baseQuery
                .Select(q => new QuestMetadata
                {
                    Id = q.Id,
                    QuestType = q.QuestType,
                    AccountId = q.AccountId,

                    // Include only the relevant quest type
                    OneTimeQuest = q.OneTimeQuest,
                    DailyQuest = q.DailyQuest,
                    WeeklyQuest = q.WeeklyQuest,
                    MonthlyQuest = q.MonthlyQuest,
                    SeasonalQuest = q.SeasonalQuest,
                    QuestLabels = q.QuestLabels
                    .Select(ql => new QuestMetadata_QuestLabel
                    {
                        QuestLabel = new QuestLabel
                        {
                            Id = ql.QuestLabelId,
                            Value = ql.QuestLabel.Value,
                            BackgroundColor = ql.QuestLabel.BackgroundColor,
                            TextColor = ql.QuestLabel.TextColor
                        }
                    }).ToList()
                })
                .AsNoTracking();

            return await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<QuestMetadata>> GetQuestsByTypeAsync(
            int accountId,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var quests = await _context.QuestsMetadata
                .Where(q => q.AccountId == accountId && q.QuestType == questType)
                .Select(q => new QuestMetadata
                {
                    Id = q.Id,
                    QuestType = q.QuestType,
                    AccountId = q.AccountId,
                    QuestLabels = q.QuestLabels
                    .Select(ql => new QuestMetadata_QuestLabel
                    {
                        QuestLabel = new QuestLabel
                        {
                            Id = ql.QuestLabelId,
                            Value = ql.QuestLabel.Value,
                            BackgroundColor = ql.QuestLabel.BackgroundColor,
                            TextColor = ql.QuestLabel.TextColor
                        }
                    }).ToList(),
                    // Only assign the relevant quest type dynamically
                    DailyQuest = q.DailyQuest,
                    WeeklyQuest = q.WeeklyQuest,
                    MonthlyQuest = q.MonthlyQuest,
                    OneTimeQuest = q.OneTimeQuest,
                    SeasonalQuest = q.SeasonalQuest
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Fetched {@quests} quests from repository.", quests);
            return quests;
        }

        public async Task<QuestMetadata?> GetQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _context.QuestsMetadata
                .Where(q => q.Id == questId)
                .Select(q => new QuestMetadata
                {
                    Id = q.Id,
                    QuestType = q.QuestType,
                    AccountId = q.AccountId,
                    QuestLabels = q.QuestLabels.Select(ql => new QuestMetadata_QuestLabel
                    {
                        QuestMetadataId = q.Id,
                        QuestLabelId = ql.QuestLabelId,
                        QuestLabel = new QuestLabel
                        {
                            Id = ql.QuestLabel.Id,
                            Value = ql.QuestLabel.Value,
                            BackgroundColor = ql.QuestLabel.BackgroundColor,
                            TextColor = ql.QuestLabel.TextColor,
                        }
                    }).ToList(),
                    DailyQuest = q.DailyQuest,
                    WeeklyQuest = q.WeeklyQuest,
                    MonthlyQuest = q.MonthlyQuest,
                    OneTimeQuest = q.OneTimeQuest,
                    SeasonalQuest = q.SeasonalQuest
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Fetched quest: {@quest}", quest);

            return quest;
        }

        public async Task<QuestMetadata?> GetQuestMetadataByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestsMetadata.FirstOrDefaultAsync(q => q.Id == questId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(QuestMetadata quest, CancellationToken cancellationToken = default)
        {
            _context.QuestsMetadata.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AddQuestLabelsAsync(List<QuestMetadata_QuestLabel> labelsToAdd, CancellationToken cancellationToken = default)
        {
            _context.QuestMetadata_QuestLabels.AddRange(labelsToAdd);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveQuestLabelsAsync(List<QuestMetadata_QuestLabel> labelsToRemove, CancellationToken cancellationToken = default)
        {
            _context.QuestMetadata_QuestLabels.RemoveRange(labelsToRemove);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsQuestOwnedByUserAsync(
            int questId,
            int accountId,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            return await _context.QuestsMetadata
                .AnyAsync(q => q.Id == questId &&
                q.AccountId == accountId &&
                q.QuestType == questType,
                cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

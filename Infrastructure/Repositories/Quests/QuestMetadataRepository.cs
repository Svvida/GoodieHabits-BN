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
                .AsQueryable();

            baseQuery = baseQuery.Where(q =>
                (q.QuestType == QuestTypeEnum.OneTime && q.OneTimeQuest != null && IsQuestActive(q.OneTimeQuest.StartDate, q.OneTimeQuest.EndDate, today)) ||
                (q.QuestType == QuestTypeEnum.Daily && q.DailyQuest != null && IsQuestActive(q.DailyQuest.StartDate, q.DailyQuest.EndDate, today)) ||
                (q.QuestType == QuestTypeEnum.Weekly && q.WeeklyQuest != null && IsQuestActive(q.WeeklyQuest.StartDate, q.WeeklyQuest.EndDate, today)) ||
                (q.QuestType == QuestTypeEnum.Monthly && q.MonthlyQuest != null && IsQuestActive(q.MonthlyQuest.StartDate, q.MonthlyQuest.EndDate, today)) ||
                (q.QuestType == QuestTypeEnum.Seasonal && q.SeasonalQuest != null && IsQuestActive(q.SeasonalQuest.StartDate, q.SeasonalQuest.EndDate, today))
            );

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

        static bool IsQuestActive(DateTime? startDate, DateTime? endDate, DateTime today)
        {
            if (startDate.HasValue && startDate.Value.Date > today)
                return false;

            if (endDate.HasValue && endDate.Value.Date < today)
                return false;

            return true;
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
    }
}

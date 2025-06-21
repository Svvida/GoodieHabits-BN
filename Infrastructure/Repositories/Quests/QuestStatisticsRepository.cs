using AutoMapper;
using Domain.Interfaces.Authentication;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestStatisticsRepository : IQuestStatisticsRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuestStatisticsRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task UpdateQuestStatisticsAsync(QuestStatistics stats, CancellationToken cancellationToken = default)
        {
            var existingStats = await _context.QuestStatistics
                .FirstOrDefaultAsync(qs => qs.Id == stats.Id).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Quest Statistics with ID: {stats.Id} not found.");

            _mapper.Map(stats, existingStats);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

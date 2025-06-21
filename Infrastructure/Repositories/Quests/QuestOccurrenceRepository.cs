using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.Quests
{
    public class QuestOccurrenceRepository : IQuestOccurrenceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestOccurrenceRepository> _logger;
        private readonly IMapper _mapper;

        public QuestOccurrenceRepository(AppDbContext context, ILogger<QuestOccurrenceRepository> logger
            , IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task SaveOccurrencesAsync(List<QuestOccurrence> occurences, CancellationToken cancellationToken = default)
        {
            foreach (var occurrence in occurences)
            {
                if (occurrence.Id != 0)
                {
                    _logger.LogWarning("Occurrence has non-zero ID before saving: {Id}", occurrence.Id);
                }
            }
            _context.QuestOccurrences.AddRange(occurences);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateOccurrence(QuestOccurrence occurence, CancellationToken cancellationToken = default)
        {
            var existingOccurrence = await _context.QuestOccurrences
                .FirstOrDefaultAsync(q => q.Id == occurence.Id, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Occurrence with ID: {occurence.Id} not found.");

            _mapper.Map(occurence, existingOccurrence);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsQuestOccurrenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .AnyAsync(q =>
                    q.QuestId == questId &&
                    q.OccurrenceStart == occurenceStart &&
                    q.OccurrenceEnd == occurenceEnd,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QuestOccurrence?> GetCurrentOccurrenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .FirstOrDefaultAsync(q =>
                    q.QuestId == questId &&
                    q.OccurrenceStart <= now &&
                    q.OccurrenceEnd >= now,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.QuestOccurrences
                .Where(q => q.QuestId == questId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

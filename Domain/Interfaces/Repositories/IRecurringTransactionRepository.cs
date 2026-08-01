using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IRecurringTransactionRepository : IBaseRepository<RecurringTransaction>
    {
        Task<RecurringTransaction?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>The user's templates, newest first.</summary>
        Task<IReadOnlyList<RecurringTransaction>> GetUserTemplatesAsync(int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Templates that actually owe transactions as of <paramref name="today"/>: active, and with a
        /// watermark earlier than the current month. Pre-filtered in SQL — mirroring
        /// <c>GetRepeatableQuestsForOccurrencesProcessingAsync</c> — so the generator never loads a template
        /// with nothing to do. Spans all users; this runs as a background sweep.
        /// </summary>
        Task<IReadOnlyList<RecurringTransaction>> GetForMaterializationAsync(DateOnly today, CancellationToken cancellationToken = default);
    }
}

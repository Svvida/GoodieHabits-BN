using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestOccurrenceRepository : IBaseRepository<QuestOccurrence>
    {
        /// <summary>
        /// Every occurrence of a single quest whose period overlaps the inclusive date range.
        /// The analytics handlers aggregate over this in memory.
        /// </summary>
        Task<List<QuestOccurrence>> GetForQuestInRangeAsync(int questId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        /// <summary>
        /// Every occurrence belonging to a user's repeating quests whose period overlaps the
        /// inclusive date range, with the owning quest loaded for grouping and labelling.
        /// </summary>
        Task<List<QuestOccurrence>> GetForUserInRangeAsync(int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elapsed periods inside the catch-up window, with their completions loaded.
        /// <para>
        /// By default only periods a tap would still change (missed or partial), so an empty result
        /// genuinely means "nothing to ask about". <paramref name="includeCompleted"/> adds the ones
        /// already ticked, which is what makes a mistaken catch-up tap undoable after an app restart —
        /// otherwise its id lives only in the response that created it.
        /// </para>
        /// </summary>
        Task<List<QuestOccurrence>> GetCatchUpCandidatesAsync(int userProfileId, DateOnly from, DateOnly to, bool includeCompleted = false, CancellationToken cancellationToken = default);
    }
}

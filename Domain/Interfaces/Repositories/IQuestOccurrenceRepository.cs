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
        /// Every occurrence belonging to a user's repeatable quests whose period overlaps the
        /// inclusive date range, with the owning quest loaded for grouping and labelling.
        /// </summary>
        Task<List<QuestOccurrence>> GetForUserInRangeAsync(int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default);
    }
}

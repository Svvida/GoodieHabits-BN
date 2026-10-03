using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestRepository : IBaseRepository<Quest>
    {
        /// <summary>
        /// Every quest whose active range covers <paramref name="today"/>, with the periods and labels a
        /// list view needs. Which of them are actually *due* today is decided in memory by
        /// <see cref="Quest.IsDueOn"/> — the schedule rules are too rich to mirror in SQL, and duplicating
        /// them there is how the two copies drift apart.
        /// </summary>
        Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(int userProfileId, DateOnly today, CancellationToken cancellationToken = default);

        /// <summary>
        /// All of a user's quests, optionally narrowed by schedule unit or by one of the retired quest types.
        /// <para>
        /// <paramref name="legacyType"/> reproduces the old buckets exactly, so the existing per-type screens
        /// keep grouping quests the way users already expect. It exists because the Daily/Weekly split moved:
        /// a "Mon/Wed/Fri" quest is now a Day schedule with a weekday filter, so plain <paramref name="unit"/>
        /// filtering would relocate it from the Weekly screen to the Daily one.
        /// </para>
        /// </summary>
        Task<IEnumerable<Quest>> GetQuestsForDisplayAsync(int userProfileId, PeriodUnitEnum? unit, QuestTypeEnum? legacyType, CancellationToken cancellationToken = default);

        Task<Quest?> GetQuestByIdAsync(int questId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loaded for writing a completion: the profile with its badges, every period, and every completion.
        /// All of the periods, not just recent ones — the entity de-duplicates against this collection in
        /// memory, and a partial load would let it emit a row that collides with the unique index.
        /// </summary>
        Task<Quest?> GetQuestForCompletionAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);

        Task<Quest?> GetQuestByIdForUpdateAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);

        Task<bool> IsQuestOwnedByUserAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int userProfileId, DateOnly today, CancellationToken cancellationToken = default);

        Task<Quest?> GetQuestWithUserProfileAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);

        Task<Quest?> GetUserQuestByIdAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);
    }
}

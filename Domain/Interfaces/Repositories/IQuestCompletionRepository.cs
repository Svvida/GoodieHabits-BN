using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestCompletionRepository : IBaseRepository<QuestCompletion>
    {
        /// <summary>
        /// One quest's completions inside an inclusive local date range. Feeds the weekday and hour-of-day
        /// breakdowns, which read the log rather than the periods so they stay meaningful for every
        /// schedule — a "three times a week" habit has one period per week but happens on days.
        /// </summary>
        Task<List<QuestCompletion>> GetForQuestInRangeAsync(int questId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        Task<List<QuestCompletion>> GetForUserInRangeAsync(int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    }
}

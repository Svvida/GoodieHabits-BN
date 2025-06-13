using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestRepository
    {
        Task<IEnumerable<Quest>> GetActiveQuestsAsync(int accountId, DateTime todayStart, DateTime todayEnd, SeasonEnum currentSeason, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsAsync(CancellationToken cancellationToken = default);
        Task DeleteQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
        Task DeleteQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        void AddQuestLabels(List<Quest_QuestLabel> labelsToAdd);
        void RemoveQuestLabels(List<Quest_QuestLabel> labelsToRemove);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task AddQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        Task UpdateQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        void AddQuestWeekdays(List<WeeklyQuest_Day> weekdaysToAdd);
        void RemoveQuestWeekdays(List<WeeklyQuest_Day> weekdaysToRemove);
        Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int accountId, DateTime now, CancellationToken cancellationToken = default);
    }
}

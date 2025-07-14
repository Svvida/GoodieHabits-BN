using Application.Dtos.Quests;
using Domain.Enum;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<BaseGetQuestDto?> GetUserQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task DeleteQuestAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetQuestEligibleForGoalAsync(int accountId, CancellationToken cancellationToken = default);
    }
}

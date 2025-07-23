using Application.Dtos.Quests;
using Domain.Enum;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetQuestEligibleForGoalAsync(int accountId, CancellationToken cancellationToken = default);
    }
}

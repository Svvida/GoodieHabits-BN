using Application.Dtos.Quests;
using Domain.Enum;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<BaseGetQuestDto?> GetUserQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<int> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task UpdateQuestCompletionAsync(BaseQuestCompletionPatchDto patchDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task DeleteQuestAsync(int questId, CancellationToken cancellationToken = default);
    }
}

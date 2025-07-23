using Application.Dtos.Quests;
using Domain.Enum;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<BaseGetQuestDto> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
    }
}

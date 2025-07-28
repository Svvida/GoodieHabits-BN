using Application.Dtos.Quests;
using Domain.Enum;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default);
    }
}

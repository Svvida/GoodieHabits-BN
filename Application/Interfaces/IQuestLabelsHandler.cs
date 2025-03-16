using Application.Dtos.Quests;
using Domain.Models;

namespace Application.Interfaces
{
    public interface IQuestLabelsHandler
    {
        Task<QuestMetadata> HandlePatchLabelsAsync(QuestMetadata quest, BaseUpdateQuestDto updateDto, CancellationToken cancellationToken = default);
    }
}

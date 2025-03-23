using Application.Dtos.Quests;
using Domain.Models;

namespace Application.Interfaces
{
    public interface IQuestLabelsHandler
    {
        Task<QuestMetadata> HandleUpdateLabelsAsync(QuestMetadata quest, BaseUpdateQuestDto updateDto, CancellationToken cancellationToken = default);
    }
}

using Application.Dtos.Quests;
using Domain.Models;

namespace Application.Interfaces
{
    public interface IQuestLabelsHandler
    {
        Task<Quest> HandleUpdateLabelsAsync(Quest quest, BaseUpdateQuestDto updateDto, CancellationToken cancellationToken = default);
    }
}

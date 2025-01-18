using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ISeasonalQuestRepository : IBaseRepository<SeasonalQuest>
    {
        Task<IEnumerable<SeasonalQuest>> GetActiveQuestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<SeasonalQuest>> GetImportantQuestsAsync(CancellationToken cancellationToken = default);

    }
}

using Domain.Models;

namespace Domain.Interfaces
{
    public interface IOneTimeQuestRepository
    {
        Task<OneTimeQuest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OneTimeQuest>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(OneTimeQuest oneTimeQuest, CancellationToken cancellationToken = default);
        Task UpdateAsync(OneTimeQuest quoneTimeQuestest, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

using Domain.Models;

namespace Domain.Interfaces
{
    public interface IRepeatableQuestRepository
    {
        public Task<RepeatableQuest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<RepeatableQuest>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task AddAsync(RepeatableQuest repeatableQuest, CancellationToken cancellationToken = default);
        public Task UpdateAsync(RepeatableQuest repeatableQuest, CancellationToken cancellationToken = default);
        public Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<RepeatableQuest>> GetByTypesAsync(List<string> types, CancellationToken cancellationToken = default);
    }
}

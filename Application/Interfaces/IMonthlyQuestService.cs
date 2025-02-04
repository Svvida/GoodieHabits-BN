using Application.Dtos.MonthlyQuest;

namespace Application.Interfaces
{
    public interface IMonthlyQuestService
    {
        Task<MonthlyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MonthlyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateMonthlyQuestDto createDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, PatchMonthlyQuestDto patchDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateMonthlyQuestDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

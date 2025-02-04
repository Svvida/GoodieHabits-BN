using Application.Dtos.DailyQuest;

namespace Application.Interfaces
{
    public interface IDailyQuestService
    {
        Task<DailyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DailyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateDailyQuestDto createDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

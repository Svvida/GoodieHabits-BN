using Application.Dtos.WeeklyQuest;

namespace Application.Interfaces
{
    public interface IWeeklyQuestService
    {
        Task<WeeklyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateWeeklyQuestDto createDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, PatchWeeklyQuestDto patchDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateWeeklyQuestDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

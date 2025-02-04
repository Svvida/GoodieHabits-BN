using Application.Dtos.SeasonalQuest;

namespace Application.Interfaces
{
    public interface ISeasonalQuestService
    {
        Task<SeasonalQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SeasonalQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateSeasonalQuestDto createDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, PatchSeasonalQuestDto patchDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateSeasonalQuestDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

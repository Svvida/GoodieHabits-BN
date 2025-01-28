using Application.Dtos.OneTimeQuest;

namespace Application.Interfaces
{
    public interface IOneTimeQuestService
    {
        Task<OneTimeQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OneTimeQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, PatchOneTimeQuestDto patchDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

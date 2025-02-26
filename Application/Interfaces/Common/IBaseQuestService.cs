namespace Application.Interfaces.Common
{
    public interface IBaseQuestService<TDto, TCreateDto, TUpdateDto, TPatchDto>
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
        where TPatchDto : class
    {
        // For admins: No account validation
        Task<TDto?> GetQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
        // For users: Validate if quest belongs to the user
        Task<TDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, TPatchDto patchDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

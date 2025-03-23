namespace Application.Interfaces.Common
{
    public interface IBaseQuestService<TDto, TCreateDto, TUpdateDto, TPatchDto>
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
        where TPatchDto : class
    {
        Task<TDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
        Task UpdateUserQuestAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task PatchUserQuestAsync(int id, TPatchDto patchDto, CancellationToken cancellationToken = default);
    }
}

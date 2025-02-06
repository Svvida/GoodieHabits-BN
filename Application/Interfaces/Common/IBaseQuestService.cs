namespace Application.Interfaces.Common
{
    public interface IBaseQuestService<TDto, TCreateDto, TUpdateDto, TPatchDto>
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
        where TPatchDto : class
    {
        Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task PatchAsync(int id, TPatchDto patchDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

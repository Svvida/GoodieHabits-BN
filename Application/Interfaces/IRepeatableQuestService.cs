using Application.Dtos.RepeatableQuest;

namespace Application.Interfaces
{
    public interface IRepeatableQuestService
    {
        public Task<RepeatableQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<RepeatableQuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task<int> CreateAsync(CreateRepeatableQuestDto createDto, CancellationToken cancellationToken = default);
        public Task PatchAsync(int id, PatchRepeatableQuestDto patchDto, CancellationToken cancellationToken = default);
        public Task UpdateAsync(int id, UpdateRepeatableQuestDto updateDto, CancellationToken cancellationToken = default);
        public Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<RepeatableQuestDto>> GetByTypesAsync(List<string> types, CancellationToken cancellationToken = default);
    }
}

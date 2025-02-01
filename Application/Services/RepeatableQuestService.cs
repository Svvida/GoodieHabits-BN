using Application.Dtos.RepeatableQuest;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class RepeatableQuestService : IRepeatableQuestService
    {
        private readonly IRepeatableQuestRepository _repository;
        private readonly IMapper _mapper;

        public RepeatableQuestService(
            IRepeatableQuestRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<RepeatableQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {id} not found.");

            return _mapper.Map<RepeatableQuestDto>(quest);
        }

        public async Task<IEnumerable<RepeatableQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<IEnumerable<RepeatableQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateRepeatableQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var repeatableQuest = _mapper.Map<RepeatableQuest>(createDto);

            await _repository.AddAsync(repeatableQuest, cancellationToken).ConfigureAwait(false);

            return repeatableQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateRepeatableQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingRepeatableQuest = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"RepeatableQuest with Id {id} was not found.");

            _mapper.Map(updateDto, existingRepeatableQuest);

            await _repository.UpdateAsync(existingRepeatableQuest, cancellationToken).ConfigureAwait(false);
        }

        public async Task PatchAsync(int id, PatchRepeatableQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingRepeatableQuest = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"RepeatableQuest with Id {id} was not found.");

            // **Fix: Manually Preserve IsCompleted Before AutoMapper Mapping**
            bool previousIsCompleted = existingRepeatableQuest.IsCompleted;

            // Apply AutoMapper Mapping (Ignores Nulls)
            _mapper.Map(patchDto, existingRepeatableQuest);

            // **Fix: Restore IsCompleted If Not Provided**
            if (patchDto.IsCompleted is null)
            {
                existingRepeatableQuest.IsCompleted = previousIsCompleted;
            }

            await _repository.UpdateAsync(existingRepeatableQuest, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<RepeatableQuestDto>> GetByTypesAsync(List<string> types, CancellationToken cancellationToken = default)
        {
            if (types is null || !types.Any())
                throw new InvalidArgumentException("At least one quest type mus be provided");

            var quests = await _repository.GetByTypesAsync(types, cancellationToken).ConfigureAwait(false);
            return _mapper.Map<IEnumerable<RepeatableQuestDto>>(quests);
        }
    }
}

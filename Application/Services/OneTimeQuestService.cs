using Application.Dtos.OneTimeQuest;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OneTimeQuestService : IOneTimeQuestService
    {
        private readonly IOneTimeQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<OneTimeQuestService> _logger;

        public OneTimeQuestService(
            IOneTimeQuestRepository repository,
            IMapper mapper,
            ILogger<OneTimeQuestService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OneTimeQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            if (quest is null)
            {
                return null;
            }
            return _mapper.Map<OneTimeQuestDto>(quest);
        }

        public async Task<IEnumerable<OneTimeQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<OneTimeQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = _mapper.Map<OneTimeQuest>(createDto);

            await _repository.AddAsync(oneTimeQuest, cancellationToken);

            return oneTimeQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            // Fetch the existing entity from the repository
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"OneTimeQuest with Id {id} was not found.");

            // Log the existing entity before update
            _logger.LogInformation("Existing OneTimeQuest before update: {@existingOneTimeQuest}", existingOneTimeQuest);

            // Map updated fields from the DTO to the existing entity
            _mapper.Map(updateDto, existingOneTimeQuest);

            // Log the updated entity after mapping
            _logger.LogInformation("Updated OneTimeQuest after mapping: {@existingOneTimeQuest}", existingOneTimeQuest);

            // Save changes to the database
            await _repository.UpdateAsync(existingOneTimeQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchOneTimeQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"OneTimeQuest with Id {id} was not found.");

            // Apply updates only if provided in the DTO
            if (patchDto.Title is not null)
            {
                existingOneTimeQuest.Title = patchDto.Title;
            }

            if (patchDto.Description is not null)
            {
                existingOneTimeQuest.Description = patchDto.Description;
            }

            if (patchDto.StartDate.HasValue)
            {
                existingOneTimeQuest.StartDate = patchDto.StartDate.Value;
            }

            if (patchDto.EndDate.HasValue)
            {
                existingOneTimeQuest.EndDate = patchDto.EndDate.Value;
            }

            if (patchDto.Emoji is not null)
            {
                existingOneTimeQuest.Emoji = patchDto.Emoji;
            }

            if (patchDto.IsCompleted.HasValue)
            {
                existingOneTimeQuest.IsCompleted = patchDto.IsCompleted.Value;
            }

            if (patchDto.Priority.HasValue)
            {
                existingOneTimeQuest.Priority = patchDto.Priority.Value;
            }

            await _repository.UpdateAsync(existingOneTimeQuest, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}

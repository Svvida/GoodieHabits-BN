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
            _logger.LogInformation("Received CreateOneTimeQuestDto with the following details: " +
                                   "Title: {Title}, Description: {Description}, StartDate: {StartDate}, EndDate: {EndDate}, " +
                                   "Emoji: {Emoji}, Priority: {Priority}, AccountId: {AccountId}",
                                   createDto.Title, createDto.Description, createDto.StartDate, createDto.EndDate,
                                   createDto.Emoji, createDto.Priority, createDto.AccountId);

            var oneTimeQuest = _mapper.Map<OneTimeQuest>(createDto);

            _logger.LogInformation("Mapped OneTimeQuest entity details: " +
                                   "Id: {Id}, Title: {Title}, Description: {Description}, StartDate: {StartDate}, " +
                                   "EndDate: {EndDate}, Emoji: {Emoji}, IsCompleted: {IsCompleted}, Priority: {Priority}, " +
                                   "AccountId: {AccountId}",
                                   oneTimeQuest.Id, oneTimeQuest.Title, oneTimeQuest.Description, oneTimeQuest.StartDate,
                                   oneTimeQuest.EndDate, oneTimeQuest.Emoji, oneTimeQuest.IsCompleted,
                                   oneTimeQuest.Priority, oneTimeQuest.Quest.AccountId);

            await _repository.AddAsync(oneTimeQuest, cancellationToken);

            _logger.LogInformation("Successfully created OneTimeQuest with Id: {Id}", oneTimeQuest.Id);

            return oneTimeQuest.Id;
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

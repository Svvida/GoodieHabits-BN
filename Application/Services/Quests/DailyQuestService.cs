using Application.Dtos.Quests.DailyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IDailyQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyQuestService> _logger;

        public DailyQuestService(
            IDailyQuestRepository repository,
            IMapper mapper,
            ILogger<DailyQuestService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GetDailyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetDailyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetDailyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetDailyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateDailyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var dailyQuest = _mapper.Map<DailyQuest>(createDto);

            dailyQuest.QuestMetadata.Id = dailyQuest.Id;
            dailyQuest.QuestMetadata.AccountId = createDto.AccountId;
            dailyQuest.QuestMetadata.QuestType = QuestTypeEnum.Daily;

            await _repository.AddAsync(dailyQuest, cancellationToken);

            return dailyQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingDailyQuest.EndDate.HasValue && updateDto.StartDate.HasValue)
            {
                if (existingDailyQuest.EndDate.Value < updateDto.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before start date.");
            }

            _mapper.Map(updateDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingDailyQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingDailyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingDailyQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingDailyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            if (existingDailyQuest.IsCompleted == false && patchDto.IsCompleted == true)
                existingDailyQuest.LastCompleted = DateTime.UtcNow;

            _mapper.Map(patchDto, existingDailyQuest);


            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}

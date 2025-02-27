﻿using Application.Dtos.Quests.DailyQuest;
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

        public async Task<GetDailyQuestDto?> GetQuestByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetDailyQuestDto>(quest);
        }

        public async Task<GetDailyQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(questId, cancellationToken, dq => dq.QuestMetadata);

            if (quest is null)
                return null;

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            return _mapper.Map<GetDailyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetDailyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetDailyQuestDto>>(quests);
        }
        public async Task<IEnumerable<GetDailyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllUserQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

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

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingDailyQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            if (existingDailyQuest.EndDate.HasValue && updateDto.StartDate.HasValue)
            {
                if (existingDailyQuest.EndDate.Value < updateDto.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before start date.");
            }

            _mapper.Map(updateDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingDailyQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

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

        public async Task DeleteUserQuestAsync(int id, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}

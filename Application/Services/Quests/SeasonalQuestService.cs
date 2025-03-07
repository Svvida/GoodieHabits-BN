﻿using Application.Dtos.Quests.SeasonalQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services.Quests
{
    public class SeasonalQuestService : ISeasonalQuestService
    {
        private readonly ISeasonalQuestRepository _repository;
        private readonly IMapper _mapper;

        public SeasonalQuestService(ISeasonalQuestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetSeasonalQuestDto?> GetQuestByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetSeasonalQuestDto>(quest);
        }
        public async Task<GetSeasonalQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(questId, cancellationToken, sq => sq.QuestMetadata);

            if (quest is null)
                return null;

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            return _mapper.Map<GetSeasonalQuestDto>(quest);
        }
        public async Task<IEnumerable<GetSeasonalQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetSeasonalQuestDto>>(quests);
        }
        public async Task<IEnumerable<GetSeasonalQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllUserQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetSeasonalQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateSeasonalQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var seasonalQuest = _mapper.Map<SeasonalQuest>(createDto);

            seasonalQuest.QuestMetadata.Id = seasonalQuest.Id;
            seasonalQuest.QuestMetadata.AccountId = createDto.AccountId;
            seasonalQuest.QuestMetadata.QuestType = QuestTypeEnum.Seasonal;

            await _repository.AddAsync(seasonalQuest, cancellationToken);

            return seasonalQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateSeasonalQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _repository.GetByIdAsync(id, cancellationToken, sq => sq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingSeasonalQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            _mapper.Map(updateDto, existingSeasonalQuest);

            await _repository.UpdateAsync(existingSeasonalQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchSeasonalQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _repository.GetByIdAsync(id, cancellationToken, sq => sq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingSeasonalQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingSeasonalQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingSeasonalQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingSeasonalQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingSeasonalQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(patchDto, existingSeasonalQuest);

            await _repository.UpdateAsync(existingSeasonalQuest, cancellationToken);
        }

        public async Task DeleteUserQuestAsync(int id, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken, sq => sq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}

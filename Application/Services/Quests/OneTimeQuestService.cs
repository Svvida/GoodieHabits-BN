﻿using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class OneTimeQuestService : IOneTimeQuestService
    {
        private readonly IOneTimeQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<OneTimeQuestService> _logger;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public OneTimeQuestService(
            IOneTimeQuestRepository repository,
            IMapper mapper,
            ILogger<OneTimeQuestService> logger,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetOneTimeQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            if (quest.QuestType != QuestTypeEnum.OneTime)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.OneTime, quest.QuestType);

            return _mapper.Map<GetOneTimeQuestDto>(quest);
        }

        public async Task<IEnumerable<GetOneTimeQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.OneTime, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetOneTimeQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = _mapper.Map<OneTimeQuest>(createDto);

            _logger.LogInformation("OneTimeQuest after mapping: {@oneTimeQuest}", oneTimeQuest);

            await _repository.AddAsync(oneTimeQuest, cancellationToken);

            return oneTimeQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (updateDto.StartDate.HasValue && existingQuest.OneTimeQuest!.EndDate.HasValue)
            {
                if (updateDto.StartDate.Value > existingQuest.OneTimeQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (updateDto.EndDate.HasValue && existingQuest.OneTimeQuest!.StartDate.HasValue)
            {
                if (updateDto.EndDate.Value < existingQuest.OneTimeQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(updateDto, existingQuest.OneTimeQuest);

            var questWithLabels = await _questLabelsHandler.HandlePatchLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.OneTimeQuest = questWithLabels.OneTimeQuest!;

            await _repository.UpdateAsync(existingQuest.OneTimeQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchOneTimeQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken, otq => otq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"OneTimeQuest with Id {id} was not found.");

            if (existingOneTimeQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingOneTimeQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingOneTimeQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingOneTimeQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingOneTimeQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(patchDto, existingOneTimeQuest);

            await _repository.UpdateAsync(existingOneTimeQuest, cancellationToken);
        }

        public async Task DeleteUserQuestAsync(int id, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestMetadataByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (quest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            await _questMetadataRepository.DeleteAsync(quest, cancellationToken);
        }
    }
}

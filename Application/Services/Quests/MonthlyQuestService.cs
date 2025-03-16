using Application.Dtos.Quests.MonthlyQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;

namespace Application.Services.Quests
{
    public class MonthlyQuestService : IMonthlyQuestService
    {
        private readonly IMonthlyQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public MonthlyQuestService(
            IMonthlyQuestRepository repository,
            IMapper mapper,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetMonthlyQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            if (quest.QuestType != QuestTypeEnum.Monthly)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.Monthly, quest.QuestType);

            return _mapper.Map<GetMonthlyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetMonthlyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Monthly, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetMonthlyQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateMonthlyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var monthlyQuest = _mapper.Map<MonthlyQuest>(createDto);

            await _repository.AddAsync(monthlyQuest, cancellationToken);

            return monthlyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateMonthlyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingQuest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (updateDto.StartDate.HasValue && existingQuest.MonthlyQuest!.EndDate.HasValue)
            {
                if (updateDto.StartDate.Value > existingQuest.MonthlyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (updateDto.EndDate.HasValue && existingQuest.MonthlyQuest!.StartDate.HasValue)
            {
                if (updateDto.EndDate.Value < existingQuest.MonthlyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(updateDto, existingQuest.MonthlyQuest);

            var questWithLabels = await _questLabelsHandler.HandlePatchLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.MonthlyQuest = questWithLabels.MonthlyQuest!;

            await _repository.UpdateAsync(existingQuest.MonthlyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchMonthlyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingMonthlyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingMonthlyQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingMonthlyQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingMonthlyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingMonthlyQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingMonthlyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(patchDto, existingMonthlyQuest);

            await _repository.UpdateAsync(existingMonthlyQuest, cancellationToken);
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

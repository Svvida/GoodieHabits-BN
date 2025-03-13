using Application.Dtos.Quests.MonthlyQuest;
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

        public MonthlyQuestService(
            IMonthlyQuestRepository repository,
            IMapper mapper,
            IQuestMetadataRepository questMetadataRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _questMetadataRepository = questMetadataRepository;
        }

        public async Task<GetMonthlyQuestDto?> GetQuestByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetMonthlyQuestDto>(quest);
        }

        public async Task<GetMonthlyQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(questId, cancellationToken, mq => mq.QuestMetadata);

            if (quest is null)
                return null;

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            return _mapper.Map<GetMonthlyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetMonthlyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetMonthlyQuestDto>>(quests);
        }
        public async Task<IEnumerable<GetMonthlyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetUserSubtypeQuestsAsync(accountId, QuestTypeEnum.Monthly, cancellationToken)
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
            var existingMonthlyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingMonthlyQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            _mapper.Map(updateDto, existingMonthlyQuest);

            await _repository.UpdateAsync(existingMonthlyQuest, cancellationToken);
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
            var quest = await _repository.GetByIdAsync(id, cancellationToken, mq => mq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}

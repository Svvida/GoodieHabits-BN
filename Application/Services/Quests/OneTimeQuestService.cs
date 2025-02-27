using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
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

        public async Task<GetOneTimeQuestDto?> GetQuestByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetOneTimeQuestDto>(quest);
        }
        public async Task<GetOneTimeQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(questId, cancellationToken, otq => otq.QuestMetadata);

            if (quest is null)
                return null;

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            return _mapper.Map<GetOneTimeQuestDto>(quest);
        }

        public async Task<IEnumerable<GetOneTimeQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetOneTimeQuestDto>>(quests);
        }
        public async Task<IEnumerable<GetOneTimeQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllUserQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetOneTimeQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = _mapper.Map<OneTimeQuest>(createDto);

            oneTimeQuest.QuestMetadata.Id = oneTimeQuest.Id;
            oneTimeQuest.QuestMetadata.QuestType = Domain.Enum.QuestTypeEnum.OneTime;
            oneTimeQuest.QuestMetadata.AccountId = oneTimeQuest.QuestMetadata.AccountId;

            await _repository.AddAsync(oneTimeQuest, cancellationToken);

            return oneTimeQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"OneTimeQuest with Id {id} was not found.");

            _mapper.Map(updateDto, existingOneTimeQuest);

            await _repository.UpdateAsync(existingOneTimeQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchOneTimeQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"OneTimeQuest with Id {id} was not found.");

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

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}

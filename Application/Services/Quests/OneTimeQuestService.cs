using Application.Dtos.Quests.OneTimeQuest;
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

        public async Task<GetOneTimeQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

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

            await _repository.AddAsync(oneTimeQuest, cancellationToken);

            return oneTimeQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.QuestType != QuestTypeEnum.OneTime)
                throw new InvalidQuestTypeException(id, QuestTypeEnum.OneTime, existingQuest.QuestType);

            existingQuest.OneTimeQuest!.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, existingQuest.OneTimeQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.OneTimeQuest = questWithLabels.OneTimeQuest!;

            await _repository.UpdateAsync(existingQuest.OneTimeQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(int id, OneTimeQuestCompletionDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _repository.GetByIdAsync(id, cancellationToken, otq => otq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"OneTimeQuest with Id {id} was not found.");

            _mapper.Map(patchDto, existingOneTimeQuest);

            await _repository.UpdateAsync(existingOneTimeQuest, cancellationToken);
        }
    }
}

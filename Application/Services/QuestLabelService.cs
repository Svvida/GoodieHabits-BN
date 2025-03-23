using Application.Dtos.Labels;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class QuestLabelService : IQuestLabelService
    {
        private readonly IQuestLabelRepository _questLabelRepository;
        private readonly IMapper _mapper;
        public QuestLabelService(
            IQuestLabelRepository questLabelRepository,
            IMapper mapper)
        {
            _questLabelRepository = questLabelRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetQuestLabelDto>> GetUserLabelsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var labels = await _questLabelRepository.GetUserLabelsAsync(accountId, cancellationToken).ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetQuestLabelDto>>(labels);
        }

        public async Task<int> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default)
        {
            if (await _questLabelRepository.GetLabelByValueAsync(createDto.Value, createDto.AccountId, cancellationToken).ConfigureAwait(false) != null)
                throw new ConflictException($"Label with value: {createDto.Value} already exists");

            var label = _mapper.Map<QuestLabel>(createDto);
            await _questLabelRepository.CreateLabelAsync(label, cancellationToken).ConfigureAwait(false);

            return label.Id;
        }

        public async Task PatchLabelAsync(int labelId, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default)
        {
            var label = await _questLabelRepository.GetLabelByIdAsync(labelId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"QuestLabel with ID: {labelId} not found");

            var mappedLabel = _mapper.Map(patchDto, label);

            await _questLabelRepository.UpdateLabelAsync(mappedLabel, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteLabelAsync(int labelId, CancellationToken cancellationToken = default)
        {
            var label = await _questLabelRepository.GetLabelByIdAsync(labelId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"QuestLabel with ID: {labelId} not found");

            await _questLabelRepository.DeleteLabelAsync(label, cancellationToken).ConfigureAwait(false);
        }
    }
}

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public QuestLabelService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.QuestLabels.GetLabelByValueAsync(createDto.Value, createDto.AccountId, cancellationToken).ConfigureAwait(false) != null)
                throw new ConflictException($"Label with value: {createDto.Value} already exists");

            var label = _mapper.Map<QuestLabel>(createDto);
            await _unitOfWork.QuestLabels.AddAsync(label, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return label.Id;
        }

        public async Task PatchLabelAsync(int labelId, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default)
        {
            var label = await _unitOfWork.QuestLabels.GetByIdAsync(labelId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"QuestLabel with ID: {labelId} not found");

            _mapper.Map(patchDto, label);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteLabelAsync(int labelId, CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.QuestLabels.ExistsByIdAsync(labelId, cancellationToken).ConfigureAwait(false))
                throw new NotFoundException($"QuestLabel with ID: {labelId} does not exist");

            await _unitOfWork.QuestLabels.ExecuteDeleteAsync(labelId, cancellationToken).ConfigureAwait(false);
        }
    }
}

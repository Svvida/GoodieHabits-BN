using Application.QuestLabels.Queries.GetUserLabels;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.Commands.PatchQuestLabel
{
    public class PatchQuestLabelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<PatchQuestLabelCommand, GetQuestLabelDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<GetQuestLabelDto> Handle(PatchQuestLabelCommand request, CancellationToken cancellationToken)
        {
            var patchDto = request.PatchDto;
            var label = await _unitOfWork.QuestLabels.GetByIdAsync(patchDto.Id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Label with ID {patchDto.Id} not found.");

            label.UpdateValue(patchDto.Value);
            label.UpdateBackgroundColor(patchDto.BackgroundColor);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<GetQuestLabelDto>(label);
        }
    }
}

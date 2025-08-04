using Application.QuestLabels.UpdateQuestLabel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.PatchQuestLabel
{
    public class UpdateQuestLabelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateQuestLabelCommand, UpdateQuestLabelResponse>
    {
        public async Task<UpdateQuestLabelResponse> Handle(UpdateQuestLabelCommand request, CancellationToken cancellationToken)
        {
            var label = await unitOfWork.QuestLabels.GetByIdAsync(request.LabelId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Label with ID {request.LabelId} not found.");

            label.UpdateValue(request.Value);
            label.UpdateBackgroundColor(request.BackgroundColor);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return mapper.Map<UpdateQuestLabelResponse>(label);
        }
    }
}

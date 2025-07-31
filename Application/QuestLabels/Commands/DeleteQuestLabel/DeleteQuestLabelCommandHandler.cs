using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public class DeleteQuestLabelCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuestLabelCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Unit> Handle(DeleteQuestLabelCommand request, CancellationToken cancellationToken)
        {
            if (!await _unitOfWork.QuestLabels.ExistsByIdAsync(request.Id, cancellationToken).ConfigureAwait(false))
                throw new NotFoundException($"QuestLabel with ID: {request.Id} does not exist");

            await _unitOfWork.QuestLabels.ExecuteDeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}

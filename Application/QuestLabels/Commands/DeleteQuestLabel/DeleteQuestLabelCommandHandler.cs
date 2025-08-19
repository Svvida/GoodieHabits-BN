using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public class DeleteQuestLabelCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuestLabelCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteQuestLabelCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.QuestLabels.ExecuteDeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}

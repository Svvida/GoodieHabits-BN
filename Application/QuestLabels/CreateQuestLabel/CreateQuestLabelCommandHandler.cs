using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.QuestLabels.CreateQuestLabel
{
    public class CreateQuestLabelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateQuestLabelCommand, CreateQuestLabelResponse>
    {
        public async Task<CreateQuestLabelResponse> Handle(CreateQuestLabelCommand request, CancellationToken cancellationToken)
        {
            var label = QuestLabel.Create(
                accountId: request.AccountId,
                value: request.Value,
                backgroundColor: request.BackgroundColor);

            await unitOfWork.QuestLabels.AddAsync(label, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<CreateQuestLabelResponse>(label);
        }
    }
}

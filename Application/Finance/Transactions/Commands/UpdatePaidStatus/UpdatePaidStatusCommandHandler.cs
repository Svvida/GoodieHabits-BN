using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Commands.UpdatePaidStatus
{
    public class UpdatePaidStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdatePaidStatusCommand, TransactionDto>
    {
        public async Task<TransactionDto> Handle(UpdatePaidStatusCommand request, CancellationToken cancellationToken)
        {
            // Corrections come along so the returned row matches what the list already shows.
            var transaction = await unitOfWork.FinanceTransactions
                .GetOwnedWithCorrectionsAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            if (transaction.IsCorrection)
                throw new ConflictException(
                    "A correction is money that has already come back, so it is always paid. " +
                    "Set the paid status on the transaction it corrects instead.");

            transaction.MarkPaid(request.IsPaid);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<TransactionDto>(transaction);
        }
    }
}

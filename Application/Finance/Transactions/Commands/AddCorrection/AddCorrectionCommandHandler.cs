using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Commands.AddCorrection
{
    public class AddCorrectionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<AddCorrectionCommand, TransactionDto>
    {
        public async Task<TransactionDto> Handle(AddCorrectionCommand request, CancellationToken cancellationToken)
        {
            // Tracked, with the existing corrections loaded so the returned DTO carries the full set.
            var parent = await unitOfWork.FinanceTransactions
                .GetOwnedWithCorrectionsAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            if (parent.IsCorrection)
                throw new ConflictException(
                    $"Transaction with ID {request.TransactionId} is itself a correction and cannot be corrected.");

            var correctable = parent.Amount - parent.CorrectedAmount;
            if (request.Amount > correctable)
                throw new ConflictException(
                    $"Corrections cannot exceed the transaction amount ({parent.Amount}); {correctable} remains correctable. " +
                    "Money received beyond the original amount is a separate income transaction.");

            parent.ApplyCorrection(request.Amount);

            var correction = FinanceTransaction.CreateCorrection(
                request.UserProfileId,
                parent,
                request.Amount,
                request.OccurredOn,
                request.Note);

            await unitOfWork.FinanceTransactions.AddAsync(correction, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<TransactionDto>(parent);
        }
    }
}

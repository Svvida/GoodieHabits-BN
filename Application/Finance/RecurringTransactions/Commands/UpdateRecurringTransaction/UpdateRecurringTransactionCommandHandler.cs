using Application.Finance.RecurringTransactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    public class UpdateRecurringTransactionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<UpdateRecurringTransactionCommand, RecurringTransactionDto>
    {
        public async Task<RecurringTransactionDto> Handle(
            UpdateRecurringTransactionCommand request, CancellationToken cancellationToken)
        {
            var template = await unitOfWork.RecurringTransactions
                .GetOwnedByIdAsync(request.RecurringTransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Recurring transaction with ID {request.RecurringTransactionId} not found.");

            if (request.Amount is decimal amount)
                template.UpdateAmount(amount);

            if (request.DayOfMonth is int dayOfMonth)
                template.UpdateDayOfMonth(dayOfMonth);

            // Note is nullable in its own right, so "clear the note" and "don't touch it" are indistinguishable
            // on a partial update. Sending an explicit empty string clears it; omitting it leaves it alone.
            if (request.Note is not null)
                template.UpdateNote(request.Note.Length == 0 ? null : request.Note);

            if (request.IsActive is bool isActive)
            {
                var today = DateOnly.FromDateTime(clock.GetCurrentInstant().ToDateTimeUtc());
                template.SetActive(isActive, today);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<RecurringTransactionDto>(template);
        }
    }
}

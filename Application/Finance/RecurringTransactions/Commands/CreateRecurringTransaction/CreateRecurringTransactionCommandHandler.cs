using Application.Finance.RecurringTransactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction
{
    public class CreateRecurringTransactionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<CreateRecurringTransactionCommand, RecurringTransactionDto>
    {
        public async Task<RecurringTransactionDto> Handle(
            CreateRecurringTransactionCommand request, CancellationToken cancellationToken)
        {
            if (request.CategoryId is int categoryId)
            {
                var category = await unitOfWork.FinanceCategories
                    .GetAssignableByIdAsync(categoryId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Category with ID {categoryId} not found.");

                if (category.Type != request.Type)
                    throw new ConflictException(
                        $"Category '{category.Name}' is a {category.Type} category and cannot be used for a {request.Type} transaction.");
            }

            // Today fixes the watermark, so the first generated row lands next month — the client adds the
            // current month's transaction itself when it wants one.
            var today = DateOnly.FromDateTime(clock.GetCurrentInstant().ToDateTimeUtc());

            var template = RecurringTransaction.Create(
                request.UserProfileId,
                request.Type,
                request.Amount,
                request.DayOfMonth,
                today,
                request.CategoryId,
                request.Note);

            await unitOfWork.RecurringTransactions.AddAsync(template, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<RecurringTransactionDto>(template);
        }
    }
}

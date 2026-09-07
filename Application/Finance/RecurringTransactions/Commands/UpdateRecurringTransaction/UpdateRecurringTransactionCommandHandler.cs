using Application.Finance.RecurringTransactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
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

            // Same ambiguity for the category, resolved by the request tracking whether the field was sent at
            // all. Only future materializations follow the new category — see RecurringTransaction.UpdateCategory.
            if (request.HasCategoryId)
                await ApplyCategoryAsync(template, request, cancellationToken).ConfigureAwait(false);

            if (request.IsActive is bool isActive)
            {
                var today = DateOnly.FromDateTime(clock.GetCurrentInstant().ToDateTimeUtc());
                template.SetActive(isActive, today);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<RecurringTransactionDto>(template);
        }

        private async Task ApplyCategoryAsync(
            RecurringTransaction template,
            UpdateRecurringTransactionCommand request,
            CancellationToken cancellationToken)
        {
            if (request.CategoryId is not int categoryId)
            {
                template.UpdateCategory(null);
                return;
            }

            var category = await unitOfWork.FinanceCategories
                .GetAssignableByIdAsync(categoryId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Category with ID {categoryId} not found.");

            // Type is immutable on a template, so the new category has to match what the template already is.
            if (category.Type != template.Type)
                throw new ConflictException(
                    $"Category '{category.Name}' is a {category.Type} category and cannot be used for a {template.Type} transaction.");

            template.UpdateCategory(categoryId);
        }
    }
}

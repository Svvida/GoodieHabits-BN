using Application.Finance.Budgets.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Budgets.Commands.CreateBudget
{
    public class CreateBudgetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateBudgetCommand, BudgetDto>
    {
        public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
        {
            if (request.CategoryId is int categoryId)
            {
                var categoryExists = await unitOfWork.FinanceCategories
                    .GetAssignableByIdAsync(categoryId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false);

                if (categoryExists is null)
                    throw new NotFoundException($"Category with ID {categoryId} not found.");
            }

            // Yearly budgets never carry a month; normalize before the uniqueness check.
            var month = request.Period == BudgetPeriodEnum.Monthly ? request.Month : null;

            var exists = await unitOfWork.Budgets
                .ExistsForPeriodAsync(request.UserProfileId, request.CategoryId, request.Period, request.Year, month, null, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
                throw new ConflictException("A budget already exists for this scope and period.");

            var budget = Budget.Create(
                request.UserProfileId,
                request.CategoryId,
                request.Period,
                request.Year,
                request.Month,
                request.LimitAmount);

            await unitOfWork.Budgets.AddAsync(budget, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<BudgetDto>(budget);
        }
    }
}

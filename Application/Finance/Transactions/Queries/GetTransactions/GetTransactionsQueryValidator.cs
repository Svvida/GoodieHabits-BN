using FluentValidation;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    public class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
    {
        public GetTransactionsQueryValidator()
        {
            RuleFor(q => q.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(q => q.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

            RuleFor(q => q)
                .Must(q => !(q.From.HasValue && q.To.HasValue) || q.From.Value <= q.To.Value)
                .WithMessage("'From' must be on or before 'To'.");
        }
    }
}

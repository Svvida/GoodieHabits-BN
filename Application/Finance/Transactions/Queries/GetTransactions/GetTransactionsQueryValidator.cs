using Domain.Models;
using FluentValidation;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    public class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
    {
        private const int MaxCategoryIds = 50;

        public GetTransactionsQueryValidator()
        {
            RuleFor(q => q.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(q => q.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

            // A bounded IN list: generous next to a realistic category tree, small enough that nobody can
            // paste ten thousand ids into the query string.
            RuleFor(q => q.CategoryIds!)
                .Must(ids => ids.Count <= MaxCategoryIds)
                .WithMessage($"No more than {MaxCategoryIds} category ids may be requested at once.")
                .When(q => q.CategoryIds is not null);

            RuleForEach(q => q.CategoryIds!)
                .GreaterThan(0).WithMessage("Category ids must be greater than 0.")
                .When(q => q.CategoryIds is not null);

            // Nothing longer than the longest thing it could match is worth sending to the database.
            RuleFor(q => q.Search)
                .MaximumLength(FinanceTransaction.NoteMaxLength)
                .WithMessage("Search must not exceed {MaxLength} characters.");

            RuleFor(q => q)
                .Must(q => !(q.From.HasValue && q.To.HasValue) || q.From.Value <= q.To.Value)
                .WithMessage("'From' must be on or before 'To'.");
        }
    }
}

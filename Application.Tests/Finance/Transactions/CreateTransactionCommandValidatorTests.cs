using Application.Finance.Transactions.Commands.CreateTransaction;
using Domain.Enums;
using Domain.Models;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Transactions
{
    public class CreateTransactionCommandValidatorTests
    {
        private readonly CreateTransactionCommandValidator _validator = new();

        private static CreateTransactionCommand Valid() =>
            new(FinanceTransactionTypeEnum.Expense, 25m, new DateOnly(2026, 1, 15), 3, "coffee", null, 1);

        [Fact]
        public void Should_Pass_ForValidCommand()
        {
            _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_WhenAmountNotPositive(decimal amount)
        {
            var command = Valid() with { Amount = amount };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Amount);
        }

        [Fact]
        public void Should_HaveError_WhenOccurredOnUnset()
        {
            var command = Valid() with { OccurredOn = default };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.OccurredOn);
        }

        [Fact]
        public void Should_HaveError_WhenNoteTooLong()
        {
            var command = Valid() with { Note = new string('x', FinanceTransaction.NoteMaxLength + 1) };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Note);
        }

        [Fact]
        public void Should_Pass_WhenCategoryIdNull()
        {
            var command = Valid() with { CategoryId = null };
            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }
    }
}

using Application.Finance.Transactions.Commands.AddCorrection;
using Domain.Models;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Transactions
{
    public class AddCorrectionCommandValidatorTests
    {
        private readonly AddCorrectionCommandValidator _validator = new();

        private static AddCorrectionCommand Valid() =>
            new(7, 300m, new DateOnly(2026, 2, 3), "friends paid back", 1);

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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_WhenTransactionIdNotPositive(int transactionId)
        {
            var command = Valid() with { TransactionId = transactionId };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.TransactionId);
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
        public void Should_Pass_WhenNoteNull()
        {
            var command = Valid() with { Note = null };
            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }
    }
}

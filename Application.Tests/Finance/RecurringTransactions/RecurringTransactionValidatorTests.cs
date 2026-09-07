using Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction;
using Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction;
using Domain.Enums;
using Domain.Models;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.RecurringTransactions
{
    /// <summary>ValidationBehavior isn't exercised when handlers are invoked directly, so validators get their own tests.</summary>
    public class RecurringTransactionValidatorTests
    {
        private readonly CreateRecurringTransactionCommandValidator _createValidator = new();
        private readonly UpdateRecurringTransactionCommandValidator _updateValidator = new();

        private static CreateRecurringTransactionCommand ValidCreate() =>
            new(FinanceTransactionTypeEnum.Expense, 40m, 10, 3, "Netflix", 1);

        private static UpdateRecurringTransactionCommand ValidUpdate() =>
            new(7, null, null, null, null, null, false, 1);

        [Fact]
        public void Create_ShouldPass_ForAValidCommand()
        {
            _createValidator.TestValidate(ValidCreate()).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_ShouldHaveError_WhenAmountNotPositive(decimal amount)
        {
            _createValidator.TestValidate(ValidCreate() with { Amount = amount })
                .ShouldHaveValidationErrorFor(c => c.Amount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(32)]
        [InlineData(-1)]
        public void Create_ShouldHaveError_WhenDayOfMonthOutOfRange(int dayOfMonth)
        {
            _createValidator.TestValidate(ValidCreate() with { DayOfMonth = dayOfMonth })
                .ShouldHaveValidationErrorFor(c => c.DayOfMonth);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(31)]
        public void Create_ShouldAcceptTheRangeBounds(int dayOfMonth)
        {
            _createValidator.TestValidate(ValidCreate() with { DayOfMonth = dayOfMonth })
                .ShouldNotHaveValidationErrorFor(c => c.DayOfMonth);
        }

        [Fact]
        public void Create_ShouldHaveError_WhenNoteTooLong()
        {
            _createValidator.TestValidate(ValidCreate() with { Note = new string('x', FinanceTransaction.NoteMaxLength + 1) })
                .ShouldHaveValidationErrorFor(c => c.Note);
        }

        [Fact]
        public void Update_ShouldPass_WhenEverythingIsOmitted()
        {
            // A partial update that changes nothing is legal; the handler simply leaves the template alone.
            _updateValidator.TestValidate(ValidUpdate()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Update_ShouldHaveError_WhenAmountSentButNotPositive()
        {
            _updateValidator.TestValidate(ValidUpdate() with { Amount = 0m })
                .ShouldHaveValidationErrorFor(c => c.Amount!.Value);
        }

        [Fact]
        public void Update_ShouldHaveError_WhenCategoryIdSentButNotPositive()
        {
            _updateValidator.TestValidate(ValidUpdate() with { CategoryId = 0, HasCategoryId = true })
                .ShouldHaveValidationErrorFor(c => c.CategoryId!.Value);
        }

        [Fact]
        public void Update_ShouldPass_WhenCategoryIdSentAsNullToClearIt()
        {
            _updateValidator.TestValidate(ValidUpdate() with { CategoryId = null, HasCategoryId = true })
                .ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Update_ShouldHaveError_WhenDayOfMonthSentButOutOfRange()
        {
            _updateValidator.TestValidate(ValidUpdate() with { DayOfMonth = 32 })
                .ShouldHaveValidationErrorFor(c => c.DayOfMonth!.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Update_ShouldHaveError_WhenIdNotPositive(int id)
        {
            _updateValidator.TestValidate(ValidUpdate() with { RecurringTransactionId = id })
                .ShouldHaveValidationErrorFor(c => c.RecurringTransactionId);
        }
    }
}

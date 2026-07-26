using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Models
{
    public class FinanceTransactionTests
    {
        private static readonly DateOnly SampleDate = new(2026, 1, 15);

        [Fact]
        public void Create_ShouldSetFields()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 42.50m, SampleDate, 7, "  lunch  ");

            transaction.UserProfileId.Should().Be(1);
            transaction.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
            transaction.Amount.Should().Be(42.50m);
            transaction.OccurredOn.Should().Be(SampleDate);
            transaction.CategoryId.Should().Be(7);
            transaction.Note.Should().Be("lunch");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Create_ShouldThrow_WhenAmountNotPositive(decimal amount)
        {
            var act = () => FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Income, amount, SampleDate);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNoteTooLong()
        {
            var note = new string('x', FinanceTransaction.NoteMaxLength + 1);
            var act = () => FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Income, 10m, SampleDate, null, note);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void UpdateAmount_ShouldThrow_WhenNotPositive()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 10m, SampleDate);
            var act = () => transaction.UpdateAmount(0);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void MutatingMethods_ShouldUpdateState()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 10m, SampleDate, 5, "note");

            transaction.UpdateAmount(99m);
            transaction.ChangeType(FinanceTransactionTypeEnum.Income);
            transaction.UpdateDate(new DateOnly(2026, 2, 1));
            transaction.Recategorize(null);
            transaction.UpdateNote(null);

            transaction.Amount.Should().Be(99m);
            transaction.Type.Should().Be(FinanceTransactionTypeEnum.Income);
            transaction.OccurredOn.Should().Be(new DateOnly(2026, 2, 1));
            transaction.CategoryId.Should().BeNull();
            transaction.Note.Should().BeNull();
        }
    }
}

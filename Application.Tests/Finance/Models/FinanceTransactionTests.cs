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
        public void CreateCorrection_ShouldInheritTypeAndCategoryFromParent()
        {
            var parent = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate, 5, "dinner");

            var correction = FinanceTransaction.CreateCorrection(1, parent, 300m, new DateOnly(2026, 2, 3), "friends paid back");

            correction.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
            correction.CategoryId.Should().Be(5);
            correction.Amount.Should().Be(300m);
            correction.OccurredOn.Should().Be(new DateOnly(2026, 2, 3));
            correction.IsCorrection.Should().BeTrue();
            correction.CorrectsTransaction.Should().BeSameAs(parent);
        }

        [Fact]
        public void CreateCorrection_ShouldThrow_WhenParentIsItselfACorrection()
        {
            var parent = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            var correction = FinanceTransaction.CreateCorrection(1, parent, 100m, SampleDate);

            var act = () => FinanceTransaction.CreateCorrection(1, correction, 10m, SampleDate);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void CreateCorrection_ShouldThrow_WhenParentBelongsToAnotherUser()
        {
            var parent = FinanceTransaction.Create(2, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);

            var act = () => FinanceTransaction.CreateCorrection(1, parent, 100m, SampleDate);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void ApplyCorrection_ShouldReduceNetAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);

            transaction.ApplyCorrection(300m);

            transaction.CorrectedAmount.Should().Be(300m);
            transaction.NetAmount.Should().Be(100m);
        }

        [Fact]
        public void ApplyCorrection_ShouldAccumulate_AcrossMultipleCorrections()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);

            transaction.ApplyCorrection(100m);
            transaction.ApplyCorrection(250m);

            transaction.CorrectedAmount.Should().Be(350m);
            transaction.NetAmount.Should().Be(50m);
        }

        [Fact]
        public void ApplyCorrection_ShouldThrow_WhenItWouldExceedTheAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(300m);

            var act = () => transaction.ApplyCorrection(101m);

            act.Should().Throw<InvalidArgumentException>();
            transaction.CorrectedAmount.Should().Be(300m);
        }

        [Fact]
        public void ApplyCorrection_ShouldAllowCorrectingTheFullAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);

            transaction.ApplyCorrection(400m);

            transaction.NetAmount.Should().Be(0m);
        }

        [Fact]
        public void RevertCorrection_ShouldRestoreNetAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(300m);

            transaction.RevertCorrection(300m);

            transaction.CorrectedAmount.Should().Be(0m);
            transaction.NetAmount.Should().Be(400m);
        }

        [Fact]
        public void RevertCorrection_ShouldThrow_WhenMoreThanCorrected()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(100m);

            var act = () => transaction.RevertCorrection(150m);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void UpdateAmount_ShouldThrow_WhenBelowCorrectedAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(300m);

            var act = () => transaction.UpdateAmount(250m);

            act.Should().Throw<InvalidArgumentException>();
            transaction.Amount.Should().Be(400m);
        }

        [Fact]
        public void UpdateAmount_ShouldAllowLoweringDownToTheCorrectedAmount()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(300m);

            transaction.UpdateAmount(300m);

            transaction.NetAmount.Should().Be(0m);
        }

        [Fact]
        public void ChangeType_ShouldThrow_WhenCorrectionsExist()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(50m);

            var act = () => transaction.ChangeType(FinanceTransactionTypeEnum.Income);

            act.Should().Throw<InvalidArgumentException>();
            transaction.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
        }

        [Fact]
        public void ChangeType_ShouldBeANoOp_WhenTypeIsUnchanged()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            transaction.ApplyCorrection(50m);

            var act = () => transaction.ChangeType(FinanceTransactionTypeEnum.Expense);

            act.Should().NotThrow();
        }

        [Fact]
        public void ChangeType_ShouldThrow_OnACorrection()
        {
            var parent = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Expense, 400m, SampleDate);
            var correction = FinanceTransaction.CreateCorrection(1, parent, 100m, SampleDate);

            var act = () => correction.ChangeType(FinanceTransactionTypeEnum.Income);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void NetAmount_ShouldEqualAmount_WhenUncorrected()
        {
            var transaction = FinanceTransaction.Create(1, FinanceTransactionTypeEnum.Income, 1000m, SampleDate);

            transaction.NetAmount.Should().Be(1000m);
            transaction.IsCorrection.Should().BeFalse();
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

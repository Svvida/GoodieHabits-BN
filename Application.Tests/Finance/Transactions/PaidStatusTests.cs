using Application.Finance.Analytics.Queries.GetMonthlySummary;
using Application.Finance.Transactions.Commands.AddCorrection;
using Application.Finance.Transactions.Commands.CreateTransaction;
using Application.Finance.Transactions.Commands.UpdatePaidStatus;
using Application.Finance.Transactions.Commands.UpdateTransaction;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Transactions
{
    /// <summary>
    /// Phase 12.1. <c>IsPaid</c> is pure metadata: it round-trips through the API but must not move a single
    /// aggregate, exactly like <c>IsSavings</c>.
    /// </summary>
    public class PaidStatusTests : TestBase<UpdatePaidStatusCommandHandler>
    {
        private static readonly DateOnly OccurredOn = new(2026, 1, 15);

        private readonly CreateTransactionCommandHandler _create;
        private readonly UpdateTransactionCommandHandler _update;
        private readonly UpdatePaidStatusCommandHandler _updatePaidStatus;
        private readonly AddCorrectionCommandHandler _addCorrection;
        private readonly GetMonthlySummaryQueryHandler _monthlySummary;

        public PaidStatusTests()
        {
            _create = new CreateTransactionCommandHandler(_unitOfWork, _mapper);
            _update = new UpdateTransactionCommandHandler(_unitOfWork, _mapper);
            _updatePaidStatus = new UpdatePaidStatusCommandHandler(_unitOfWork, _mapper);
            _addCorrection = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
            _monthlySummary = new GetMonthlySummaryQueryHandler(_unitOfWork);
        }

        [Fact]
        public async Task Create_ShouldDefaultToPaid_WhenTheFlagIsOmitted()
        {
            var profile = await ArrangeProfileAsync();

            var result = await _create.Handle(
                new CreateTransactionCommand(FinanceTransactionTypeEnum.Expense, 50m, OccurredOn, null, "bill", null, profile.Id),
                CancellationToken.None);

            result.IsPaid.Should().BeTrue();
        }

        [Fact]
        public async Task Create_ShouldHonourAnExplicitUnpaidFlag()
        {
            var profile = await ArrangeProfileAsync();

            var result = await _create.Handle(
                new CreateTransactionCommand(FinanceTransactionTypeEnum.Expense, 50m, OccurredOn, null, "bill", false, profile.Id),
                CancellationToken.None);

            result.IsPaid.Should().BeFalse();
        }

        [Fact]
        public async Task PaidStatus_ShouldRoundTrip()
        {
            var (profile, transaction) = await ArrangeUnpaidExpenseAsync();

            var marked = await _updatePaidStatus.Handle(
                new UpdatePaidStatusCommand(transaction.Id, true, profile.Id), CancellationToken.None);

            marked.IsPaid.Should().BeTrue();

            var unmarked = await _updatePaidStatus.Handle(
                new UpdatePaidStatusCommand(transaction.Id, false, profile.Id), CancellationToken.None);

            unmarked.IsPaid.Should().BeFalse();
        }

        [Fact]
        public async Task PaidStatus_ShouldNotLeakAcrossUsers()
        {
            var (_, transaction) = await ArrangeUnpaidExpenseAsync();
            var otherAccount = await AddAccountAsync("other@test.com", "pass", "other");

            var act = async () => await _updatePaidStatus.Handle(
                new UpdatePaidStatusCommand(transaction.Id, true, otherAccount.Profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task PaidStatus_ShouldConflict_OnACorrection()
        {
            var (profile, parent) = await ArrangeUnpaidExpenseAsync();
            var corrected = await _addCorrection.Handle(
                new AddCorrectionCommand(parent.Id, 20m, OccurredOn, "refund", profile.Id), CancellationToken.None);

            var correctionId = corrected.Corrections.Single().Id;

            var act = async () => await _updatePaidStatus.Handle(
                new UpdatePaidStatusCommand(correctionId, false, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Update_ShouldTreatAnOmittedFlagAsPaid()
        {
            var (profile, transaction) = await ArrangeUnpaidExpenseAsync();

            // PUT is full replacement, so omitting the flag resets it — the same default as create.
            var result = await _update.Handle(
                new UpdateTransactionCommand(transaction.Id, FinanceTransactionTypeEnum.Expense, 50m, OccurredOn, null, "bill", null, profile.Id),
                CancellationToken.None);

            result.IsPaid.Should().BeTrue();
        }

        [Fact]
        public async Task Update_ShouldCarryAnExplicitUnpaidFlag()
        {
            var (profile, transaction) = await ArrangeUnpaidExpenseAsync();

            var result = await _update.Handle(
                new UpdateTransactionCommand(transaction.Id, FinanceTransactionTypeEnum.Expense, 50m, OccurredOn, null, "bill", false, profile.Id),
                CancellationToken.None);

            result.IsPaid.Should().BeFalse();
        }

        [Fact]
        public async Task UnpaidExpenses_ShouldStillCountTowardTheMonthlyTotals()
        {
            var (profile, _) = await ArrangeUnpaidExpenseAsync();

            var summary = await _monthlySummary.Handle(
                new GetMonthlySummaryQuery(profile.Id, 2026, 1), CancellationToken.None);

            // The whole point of the decision: a planned payment is money already spoken for.
            summary.TotalExpense.Should().Be(50m);
        }

        [Fact]
        public async Task Corrections_ShouldBeCreatedPaid()
        {
            var (profile, parent) = await ArrangeUnpaidExpenseAsync();

            var corrected = await _addCorrection.Handle(
                new AddCorrectionCommand(parent.Id, 20m, OccurredOn, "refund", profile.Id), CancellationToken.None);

            corrected.Corrections.Single().IsPaid.Should().BeTrue();
        }

        private async Task<UserProfile> ArrangeProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<(UserProfile Profile, FinanceTransaction Transaction)> ArrangeUnpaidExpenseAsync()
        {
            var profile = await ArrangeProfileAsync();

            var transaction = FinanceTransaction.Create(
                profile.Id, FinanceTransactionTypeEnum.Expense, 50m, OccurredOn, null, "bill", isPaid: false);
            _context.FinanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return (profile, transaction);
        }
    }
}

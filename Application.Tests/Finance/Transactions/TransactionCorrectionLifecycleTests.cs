using Application.Finance.Transactions.Commands.AddCorrection;
using Application.Finance.Transactions.Commands.DeleteTransaction;
using Application.Finance.Transactions.Commands.UpdateTransaction;
using Application.Finance.Transactions.Queries.GetTransactions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Finance.Transactions
{
    /// <summary>
    /// Updating and deleting either side of a correction pair has to keep the materialized
    /// <c>CorrectedAmount</c> on the parent in step. That is the whole risk of storing derived state.
    /// </summary>
    public class TransactionCorrectionLifecycleTests : TestBase<UpdateTransactionCommandHandler>
    {
        private static readonly DateOnly Dinner = new(2026, 1, 15);
        private static readonly DateOnly PaidBack = new(2026, 2, 3);

        private readonly AddCorrectionCommandHandler _addCorrection;
        private readonly UpdateTransactionCommandHandler _update;
        private readonly DeleteTransactionCommandHandler _delete;
        private readonly GetTransactionsQueryHandler _list;

        public TransactionCorrectionLifecycleTests()
        {
            _addCorrection = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
            _update = new UpdateTransactionCommandHandler(_unitOfWork, _mapper);
            _delete = new DeleteTransactionCommandHandler(_unitOfWork);
            _list = new GetTransactionsQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task UpdatingACorrectionsAmount_ShouldRecomputeTheParent()
        {
            var (profile, parent, correction) = await ArrangeCorrectedDinnerAsync();

            await _update.Handle(
                new UpdateTransactionCommand(correction.Id, correction.Type, 120m, PaidBack, correction.CategoryId, null, null, profile.Id),
                CancellationToken.None);

            var reloaded = await _context.FinanceTransactions.SingleAsync(t => t.Id == parent.Id);
            reloaded.CorrectedAmount.Should().Be(120m);
            reloaded.NetAmount.Should().Be(280m);
        }

        [Fact]
        public async Task UpdatingACorrection_ShouldThrowConflict_WhenTypeIsChanged()
        {
            var (profile, _, correction) = await ArrangeCorrectedDinnerAsync();

            var act = () => _update.Handle(
                new UpdateTransactionCommand(correction.Id, FinanceTransactionTypeEnum.Income, 300m, PaidBack, correction.CategoryId, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task UpdatingACorrection_ShouldThrowConflict_WhenCategoryIsChanged()
        {
            var (profile, _, correction) = await ArrangeCorrectedDinnerAsync();

            var act = () => _update.Handle(
                new UpdateTransactionCommand(correction.Id, correction.Type, 300m, PaidBack, 9002, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task UpdatingACorrection_ShouldSucceed_WhenTheClientEchoesTheInheritedValuesBack()
        {
            var (profile, _, correction) = await ArrangeCorrectedDinnerAsync();

            var result = await _update.Handle(
                new UpdateTransactionCommand(correction.Id, correction.Type, 300m, new DateOnly(2026, 3, 1), correction.CategoryId, "note", null, profile.Id),
                CancellationToken.None);

            result.Note.Should().Be("note");
            result.OccurredOn.Should().Be(new DateOnly(2026, 3, 1));
        }

        [Fact]
        public async Task UpdatingACorrection_ShouldThrowConflict_WhenTheNewAmountOverCorrects()
        {
            var (profile, _, correction) = await ArrangeCorrectedDinnerAsync();

            var act = () => _update.Handle(
                new UpdateTransactionCommand(correction.Id, correction.Type, 401m, PaidBack, correction.CategoryId, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task UpdatingTheParent_ShouldThrowConflict_WhenTypeIsChangedWhileCorrectionsExist()
        {
            var (profile, parent, _) = await ArrangeCorrectedDinnerAsync();

            var act = () => _update.Handle(
                new UpdateTransactionCommand(parent.Id, FinanceTransactionTypeEnum.Income, 400m, Dinner, null, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task UpdatingTheParent_ShouldThrowConflict_WhenAmountDropsBelowWhatCameBack()
        {
            var (profile, parent, _) = await ArrangeCorrectedDinnerAsync();

            var act = () => _update.Handle(
                new UpdateTransactionCommand(parent.Id, parent.Type, 250m, Dinner, parent.CategoryId, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task RecategorizingTheParent_ShouldCascadeToItsCorrections()
        {
            var (profile, parent, correction) = await ArrangeCorrectedDinnerAsync();

            await _update.Handle(
                new UpdateTransactionCommand(parent.Id, parent.Type, 400m, Dinner, null, null, null, profile.Id),
                CancellationToken.None);

            var reloaded = await _context.FinanceTransactions.SingleAsync(t => t.Id == correction.Id);
            reloaded.CategoryId.Should().BeNull();
        }

        [Fact]
        public async Task DeletingACorrection_ShouldGiveTheMoneyBackToTheParent()
        {
            var (profile, parent, correction) = await ArrangeCorrectedDinnerAsync();

            await _delete.Handle(new DeleteTransactionCommand(correction.Id, profile.Id), CancellationToken.None);

            var reloaded = await _context.FinanceTransactions.SingleAsync(t => t.Id == parent.Id);
            reloaded.CorrectedAmount.Should().Be(0m);
            reloaded.NetAmount.Should().Be(400m);
            (await _context.FinanceTransactions.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeletingTheParent_ShouldRemoveItsCorrectionsToo()
        {
            var (profile, parent, _) = await ArrangeCorrectedDinnerAsync();

            await _delete.Handle(new DeleteTransactionCommand(parent.Id, profile.Id), CancellationToken.None);

            (await _context.FinanceTransactions.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ListingTransactions_ShouldEmbedCorrectionsAndCountParentsOnly()
        {
            var (profile, parent, _) = await ArrangeCorrectedDinnerAsync();

            var page = await _list.Handle(
                new GetTransactionsQuery(profile.Id, null, null, null, null, 1, 20), CancellationToken.None);

            page.TotalCount.Should().Be(1);
            page.Items.Should().ContainSingle();
            page.Items.Single().Id.Should().Be(parent.Id);
            page.Items.Single().NetAmount.Should().Be(100m);
            page.Items.Single().Corrections.Should().ContainSingle();
        }

        [Fact]
        public async Task ListingTransactions_ShouldKeepACorrectionWithItsParent_EvenWhenTheFilterExcludesTheCorrectionsOwnDate()
        {
            var (profile, parent, _) = await ArrangeCorrectedDinnerAsync();

            // January only — the correction is dated in February.
            var page = await _list.Handle(
                new GetTransactionsQuery(profile.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), null, null, 1, 20),
                CancellationToken.None);

            page.Items.Should().ContainSingle();
            page.Items.Single().Id.Should().Be(parent.Id);
            page.Items.Single().Corrections.Should().ContainSingle();
        }

        private async Task<(UserProfile Profile, FinanceTransaction Parent, FinanceTransaction Correction)> ArrangeCorrectedDinnerAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            var profile = account.Profile;

            var category = FinanceCategory.CreateMain(profile.Id, "Food", FinanceTransactionTypeEnum.Expense);
            category.Id = 9001;
            _context.FinanceCategories.Add(category);

            var parent = FinanceTransaction.Create(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, category.Id, "dinner");
            _context.FinanceTransactions.Add(parent);
            await _context.SaveChangesAsync();

            await _addCorrection.Handle(
                new AddCorrectionCommand(parent.Id, 300m, PaidBack, "paid back", profile.Id), CancellationToken.None);

            var correction = await _context.FinanceTransactions.SingleAsync(t => t.CorrectsTransactionId == parent.Id);
            return (profile, parent, correction);
        }
    }
}

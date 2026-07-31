using Application.Finance.Transactions.Commands.AddCorrection;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Finance.Transactions
{
    public class AddCorrectionCommandHandlerTests : TestBase<AddCorrectionCommandHandler>
    {
        private static readonly DateOnly Dinner = new(2026, 1, 15);
        private static readonly DateOnly PaidBack = new(2026, 2, 3);

        private readonly AddCorrectionCommandHandler _handler;

        public AddCorrectionCommandHandlerTests()
        {
            _handler = new AddCorrectionCommandHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldNetTheParentAndReturnItWithTheCorrectionEmbedded()
        {
            var profile = await CreateProfileAsync();
            var category = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            var dinner = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, category.Id);

            var result = await _handler.Handle(
                new AddCorrectionCommand(dinner.Id, 300m, PaidBack, "friends paid back", profile.Id), CancellationToken.None);

            result.Id.Should().Be(dinner.Id);
            result.Amount.Should().Be(400m);
            result.CorrectedAmount.Should().Be(300m);
            result.NetAmount.Should().Be(100m);
            result.Corrections.Should().ContainSingle();
            result.Corrections[0].Amount.Should().Be(300m);
            result.Corrections[0].OccurredOn.Should().Be(PaidBack);
            result.Corrections[0].CorrectsTransactionId.Should().Be(dinner.Id);
        }

        [Fact]
        public async Task Handle_ShouldMakeTheCorrectionInheritTypeAndCategory()
        {
            var profile = await CreateProfileAsync();
            var category = await AddCategoryAsync(profile.Id, FinanceTransactionTypeEnum.Expense);
            var dinner = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, category.Id);

            await _handler.Handle(new AddCorrectionCommand(dinner.Id, 300m, PaidBack, null, profile.Id), CancellationToken.None);

            var correction = await _context.FinanceTransactions.SingleAsync(t => t.CorrectsTransactionId == dinner.Id);
            correction.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
            correction.CategoryId.Should().Be(category.Id);
        }

        [Fact]
        public async Task Handle_ShouldAccumulate_AcrossPartialCorrections()
        {
            var profile = await CreateProfileAsync();
            var dinner = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, null);

            await _handler.Handle(new AddCorrectionCommand(dinner.Id, 100m, PaidBack, null, profile.Id), CancellationToken.None);
            var result = await _handler.Handle(new AddCorrectionCommand(dinner.Id, 250m, PaidBack, null, profile.Id), CancellationToken.None);

            result.CorrectedAmount.Should().Be(350m);
            result.NetAmount.Should().Be(50m);
            result.Corrections.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenOverCorrecting()
        {
            var profile = await CreateProfileAsync();
            var dinner = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, null);

            var act = () => _handler.Handle(new AddCorrectionCommand(dinner.Id, 401m, PaidBack, null, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
            (await _context.FinanceTransactions.SingleAsync(t => t.Id == dinner.Id)).CorrectedAmount.Should().Be(0m);
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenCorrectingACorrection()
        {
            var profile = await CreateProfileAsync();
            var dinner = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, null);
            await _handler.Handle(new AddCorrectionCommand(dinner.Id, 300m, PaidBack, null, profile.Id), CancellationToken.None);

            var correction = await _context.FinanceTransactions.SingleAsync(t => t.CorrectsTransactionId == dinner.Id);

            var act = () => _handler.Handle(new AddCorrectionCommand(correction.Id, 10m, PaidBack, null, profile.Id), CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenTransactionBelongsToAnotherUser()
        {
            var owner = await CreateProfileAsync();
            var stranger = await CreateProfileAsync("other@test.com", "other");
            var dinner = await AddTransactionAsync(owner.Id, FinanceTransactionTypeEnum.Expense, 400m, Dinner, null);

            var act = () => _handler.Handle(new AddCorrectionCommand(dinner.Id, 100m, PaidBack, null, stranger.Id), CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldWorkOnIncome_ForTheMirrorDirection()
        {
            var profile = await CreateProfileAsync();
            var salary = await AddTransactionAsync(profile.Id, FinanceTransactionTypeEnum.Income, 5000m, Dinner, null);

            var result = await _handler.Handle(
                new AddCorrectionCommand(salary.Id, 500m, PaidBack, "overpayment returned", profile.Id), CancellationToken.None);

            result.NetAmount.Should().Be(4500m);
            result.Corrections[0].Type.Should().Be(FinanceTransactionTypeEnum.Income);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private async Task<FinanceCategory> AddCategoryAsync(int userProfileId, FinanceTransactionTypeEnum type)
        {
            var category = FinanceCategory.CreateMain(userProfileId, "Food", type);
            category.Id = 9001;
            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        private async Task<FinanceTransaction> AddTransactionAsync(
            int userProfileId, FinanceTransactionTypeEnum type, decimal amount, DateOnly occurredOn, int? categoryId)
        {
            var transaction = FinanceTransaction.Create(userProfileId, type, amount, occurredOn, categoryId);
            _context.FinanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
    }
}

using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Finance.Models
{
    public class FinanceCategoryTests
    {
        [Fact]
        public void CreateMain_ShouldCreateExpenseMain()
        {
            var category = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense, "#4E79A7", "home");

            category.IsMain.Should().BeTrue();
            category.ParentCategoryId.Should().BeNull();
            category.UserProfileId.Should().Be(1);
            category.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
            category.IsSystem.Should().BeFalse();
            category.Color.Should().Be("#4E79A7");
        }

        [Fact]
        public void CreateMain_ShouldThrow_WhenUserProfileIdInvalid()
        {
            var act = () => FinanceCategory.CreateMain(0, "Home", FinanceTransactionTypeEnum.Expense);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateMain_ShouldThrow_WhenNameEmpty(string name)
        {
            var act = () => FinanceCategory.CreateMain(1, name, FinanceTransactionTypeEnum.Expense);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void CreateMain_ShouldThrow_WhenNameTooLong()
        {
            var name = new string('a', FinanceCategory.NameMaxLength + 1);
            var act = () => FinanceCategory.CreateMain(1, name, FinanceTransactionTypeEnum.Expense);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Theory]
        [InlineData("4E79A7")]    // missing '#'
        [InlineData("#12345")]    // too short
        [InlineData("#1234567")]  // too long
        public void CreateMain_ShouldThrow_WhenColorInvalid(string color)
        {
            var act = () => FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense, color);
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void CreateSub_ShouldInheritParentTypeAndReferenceParent()
        {
            var parent = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense);
            parent.Id = 5;

            var sub = FinanceCategory.CreateSub(1, parent, "Rent");

            sub.ParentCategoryId.Should().Be(5);
            sub.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
            sub.IsMain.Should().BeFalse();
        }

        [Fact]
        public void CreateSub_UnderSystemParent_IsAllowedAndOwnedByUser()
        {
            var system = FinanceCategory.CreateSystemMain(1, "Home", FinanceTransactionTypeEnum.Expense);

            var sub = FinanceCategory.CreateSub(42, system, "Netflix");

            sub.UserProfileId.Should().Be(42);
            sub.ParentCategoryId.Should().Be(1);
            sub.Type.Should().Be(FinanceTransactionTypeEnum.Expense);
        }

        [Fact]
        public void CreateSub_ShouldThrow_WhenNestingDeeperThanOneLevel()
        {
            var parent = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense);
            parent.Id = 5;
            var sub = FinanceCategory.CreateSub(1, parent, "Rent");
            sub.Id = 6;

            var act = () => FinanceCategory.CreateSub(1, sub, "Deeper");
            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void CreateSub_ShouldThrow_WhenParentOwnedByAnotherUser()
        {
            var parent = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense);
            parent.Id = 5;

            var act = () => FinanceCategory.CreateSub(2, parent, "Rent");
            act.Should().Throw<ForbiddenException>();
        }

        [Fact]
        public void CreateMain_ShouldDefaultToNotSavings()
        {
            var category = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense);
            category.IsSavings.Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateSub_ShouldInheritIsSavingsFromParent(bool parentIsSavings)
        {
            var parent = FinanceCategory.CreateMain(1, "Savings", FinanceTransactionTypeEnum.Expense, isSavings: parentIsSavings);
            parent.Id = 5;

            var sub = FinanceCategory.CreateSub(1, parent, "Emergency fund");

            sub.IsSavings.Should().Be(parentIsSavings);
        }

        [Fact]
        public void CreateSystemSub_ShouldInheritIsSavingsFromParent()
        {
            var parent = FinanceCategory.CreateSystemMain(1, "Savings", FinanceTransactionTypeEnum.Expense, isSavings: true);

            var sub = FinanceCategory.CreateSystemSub(2, parent, "Emergency fund");

            sub.IsSavings.Should().BeTrue();
        }

        [Fact]
        public void UpdateIsSavings_ShouldToggleFlag()
        {
            var category = FinanceCategory.CreateMain(1, "Savings", FinanceTransactionTypeEnum.Expense);

            category.UpdateIsSavings(true);
            category.IsSavings.Should().BeTrue();

            category.UpdateIsSavings(false);
            category.IsSavings.Should().BeFalse();
        }

        [Fact]
        public void Rename_ShouldTrimAndUpdate()
        {
            var category = FinanceCategory.CreateMain(1, "Home", FinanceTransactionTypeEnum.Expense);
            category.Rename("  House  ");
            category.Name.Should().Be("House");
        }
    }
}

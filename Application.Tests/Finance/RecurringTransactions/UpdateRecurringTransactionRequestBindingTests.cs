using System.Text.Json;
using Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction;
using FluentAssertions;

namespace Application.Tests.Finance.RecurringTransactions
{
    /// <summary>
    /// The partial-update contract hinges on telling "categoryId": null apart from an omitted key, which only
    /// works if the deserializer really does touch the property. Pinned here rather than discovered in prod.
    /// </summary>
    public class UpdateRecurringTransactionRequestBindingTests
    {
        // Matches what ASP.NET Core uses for request bodies.
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        [Fact]
        public void OmittedCategoryId_ShouldNotBeFlaggedAsSent()
        {
            var request = JsonSerializer.Deserialize<UpdateRecurringTransactionRequest>(
                """{ "amount": 55 }""", Options)!;

            request.HasCategoryId.Should().BeFalse();
            request.CategoryId.Should().BeNull();
        }

        [Fact]
        public void ExplicitNullCategoryId_ShouldBeFlaggedAsSent()
        {
            var request = JsonSerializer.Deserialize<UpdateRecurringTransactionRequest>(
                """{ "categoryId": null }""", Options)!;

            request.HasCategoryId.Should().BeTrue();
            request.CategoryId.Should().BeNull();
        }

        [Fact]
        public void CategoryIdWithAValue_ShouldBeFlaggedAsSent()
        {
            var request = JsonSerializer.Deserialize<UpdateRecurringTransactionRequest>(
                """{ "categoryId": 42 }""", Options)!;

            request.HasCategoryId.Should().BeTrue();
            request.CategoryId.Should().Be(42);
        }

        [Fact]
        public void HasCategoryIdInTheBody_ShouldBeIgnored()
        {
            var request = JsonSerializer.Deserialize<UpdateRecurringTransactionRequest>(
                """{ "hasCategoryId": true }""", Options)!;

            request.HasCategoryId.Should().BeFalse();
        }
    }
}

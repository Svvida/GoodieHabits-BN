using System.Text.Json.Serialization;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    /// <summary>
    /// Partial update — every field is optional and an omitted one is left unchanged. This deliberately differs
    /// from <c>PUT /transactions/{id}</c> (full replacement) and matches <c>UpdateBudgetRequest</c>; the client
    /// contract was written this way and it is the friendlier shape for a settings-style resource.
    /// </summary>
    public record UpdateRecurringTransactionRequest
    {
        private readonly int? _categoryId;

        public decimal? Amount { get; init; }

        public string? Note { get; init; }

        public int? DayOfMonth { get; init; }

        public bool? IsActive { get; init; }

        /// <summary>
        /// Presence-tracked, because a bare <c>int?</c> cannot tell "clear the category" from "leave it alone":
        /// sending <c>"categoryId": null</c> clears it, omitting the key entirely leaves it as it was. Same
        /// intent as the empty-string convention on <see cref="Note"/>, expressed the way JSON allows for an id.
        /// </summary>
        public int? CategoryId
        {
            get => _categoryId;
            init
            {
                _categoryId = value;
                HasCategoryId = true;
            }
        }

        /// <summary>Set by the deserializer touching <see cref="CategoryId"/>; never read off the wire.</summary>
        [JsonIgnore]
        public bool HasCategoryId { get; private init; }
    }
}

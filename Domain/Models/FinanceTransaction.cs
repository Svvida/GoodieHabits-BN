using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A single income or expense record, discriminated by <see cref="Type"/>.
    /// <see cref="Amount"/> is always positive; the sign is implied by the type.
    /// <see cref="OccurredOn"/> is a calendar date (no time / timezone) — the day the money moved.
    /// </summary>
    public class FinanceTransaction : EntityBase
    {
        public const int NoteMaxLength = 250;

        public int Id { get; set; }
        public int UserProfileId { get; private set; }
        public FinanceTransactionTypeEnum Type { get; private set; }
        public decimal Amount { get; private set; }
        public int? CategoryId { get; private set; }
        public DateOnly OccurredOn { get; private set; }
        public string? Note { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public FinanceCategory? Category { get; set; }

        protected FinanceTransaction() { }

        private FinanceTransaction(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            DateOnly occurredOn,
            int? categoryId,
            string? note)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidateAmount(amount);
            ValidateNote(note);

            UserProfileId = userProfileId;
            Type = type;
            Amount = amount;
            OccurredOn = occurredOn;
            CategoryId = categoryId;
            Note = note?.Trim();
        }

        public static FinanceTransaction Create(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            DateOnly occurredOn,
            int? categoryId = null,
            string? note = null)
            => new(userProfileId, type, amount, occurredOn, categoryId, note);

        public void UpdateAmount(decimal amount)
        {
            ValidateAmount(amount);
            Amount = amount;
        }

        public void UpdateDate(DateOnly occurredOn) => OccurredOn = occurredOn;

        public void Recategorize(int? categoryId) => CategoryId = categoryId;

        public void ChangeType(FinanceTransactionTypeEnum type) => Type = type;

        public void UpdateNote(string? note)
        {
            ValidateNote(note);
            Note = note?.Trim();
        }

        private static void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }
    }
}

using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A single income or expense record, discriminated by <see cref="Type"/>.
    /// <see cref="Amount"/> is always positive; the sign is implied by the type.
    /// <see cref="OccurredOn"/> is a calendar date (no time / timezone) — the day the money moved.
    /// <para>
    /// Money coming back against an earlier transaction (refund, payback, reimbursement) is modelled as a
    /// <em>correction</em>: another <see cref="FinanceTransaction"/> pointing at its parent via
    /// <see cref="CorrectsTransactionId"/> and inheriting the parent's <see cref="Type"/> and
    /// <see cref="CategoryId"/> — the link carries the direction, not the type. The netting is materialized on
    /// the parent as <see cref="CorrectedAmount"/> so that analytics only ever reads <see cref="NetAmount"/>.
    /// </para>
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

        /// <summary>
        /// Whether the money has actually moved yet — mainly meaningful for expenses dated today or later.
        /// <para>
        /// Deliberately <em>pure metadata</em>, exactly like <see cref="FinanceCategory.IsSavings"/>: an unpaid
        /// transaction is aggregated as if it were already paid, so totals, breakdowns, budget progress, trend
        /// and the opening balance all treat paid and unpaid alike. A logged-but-unpaid bill is money already
        /// spoken for; excluding it would make the dashboard optimistic on precisely the days it needs to be
        /// accurate. Presentation is the client's call.
        /// </para>
        /// </summary>
        public bool IsPaid { get; private set; } = true;

        /// <summary>
        /// The recurring template that produced this row, or null when it was entered by hand. Provenance only
        /// — a materialized row is an ordinary transaction in every other respect, and survives the template
        /// being deleted (the delete handler clears this).
        /// </summary>
        public int? RecurringTransactionId { get; private set; }

        /// <summary>The transaction this one corrects, or null when this is an ordinary transaction.</summary>
        public int? CorrectsTransactionId { get; private set; }

        /// <summary>
        /// Sum of the corrections raised against this transaction. Always <c>0</c> on a correction, since a
        /// correction cannot itself be corrected. Invariant: <c>0 &lt;= CorrectedAmount &lt;= Amount</c>.
        /// </summary>
        public decimal CorrectedAmount { get; private set; }

        /// <summary>The economically effective amount — what every analytics endpoint aggregates.</summary>
        public decimal NetAmount => Amount - CorrectedAmount;

        public bool IsCorrection => CorrectsTransactionId is not null;

        public UserProfile UserProfile { get; set; } = null!;
        public FinanceCategory? Category { get; set; }
        public RecurringTransaction? RecurringTransaction { get; set; }
        public FinanceTransaction? CorrectsTransaction { get; set; }
        public ICollection<FinanceTransaction> Corrections { get; set; } = [];

        protected FinanceTransaction() { }

        private FinanceTransaction(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            DateOnly occurredOn,
            int? categoryId,
            string? note,
            bool isPaid = true,
            FinanceTransaction? correctsTransaction = null)
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
            IsPaid = isPaid;

            if (correctsTransaction is not null)
            {
                CorrectsTransactionId = correctsTransaction.Id;
                CorrectsTransaction = correctsTransaction;
            }
        }

        public static FinanceTransaction Create(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            DateOnly occurredOn,
            int? categoryId = null,
            string? note = null,
            bool isPaid = true)
            => new(userProfileId, type, amount, occurredOn, categoryId, note, isPaid);

        /// <summary>
        /// Creates a correction against <paramref name="parent"/>. <see cref="Type"/> and <see cref="CategoryId"/>
        /// are copied from the parent — the caller never supplies them, because the link, not the type, is what
        /// carries the fact that the money flowed the other way. Registering the amount on the parent is the
        /// caller's job (<see cref="ApplyCorrection"/>), so that both happen in one unit of work.
        /// <para><see cref="IsPaid"/> is always <c>true</c> here — money that has come back has come back.</para>
        /// </summary>
        public static FinanceTransaction CreateCorrection(
            int userProfileId,
            FinanceTransaction parent,
            decimal amount,
            DateOnly occurredOn,
            string? note = null)
        {
            ArgumentNullException.ThrowIfNull(parent);

            if (parent.IsCorrection)
                throw new InvalidArgumentException("A correction cannot itself be corrected.");

            if (parent.UserProfileId != userProfileId)
                throw new InvalidArgumentException("A correction must belong to the same user as the transaction it corrects.");

            return new FinanceTransaction(
                userProfileId,
                parent.Type,
                amount,
                occurredOn,
                parent.CategoryId,
                note,
                isPaid: true,
                parent);
        }

        /// <summary>
        /// Materializes <paramref name="template"/> into a real transaction for <paramref name="occurredOn"/>.
        /// Expenses start <b>unpaid</b> — the money genuinely has not moved yet, and that is what gives the
        /// client an "upcoming bills" view for free. Income starts paid, where the distinction has no meaning.
        /// </summary>
        public static FinanceTransaction CreateRecurring(RecurringTransaction template, DateOnly occurredOn)
        {
            ArgumentNullException.ThrowIfNull(template);

            var transaction = new FinanceTransaction(
                template.UserProfileId,
                template.Type,
                template.Amount,
                occurredOn,
                template.CategoryId,
                template.Note,
                isPaid: template.Type != FinanceTransactionTypeEnum.Expense);

            transaction.RecurringTransactionId = template.Id;
            transaction.RecurringTransaction = template;

            return transaction;
        }

        public void MarkPaid(bool isPaid) => IsPaid = isPaid;

        /// <summary>Detaches this row from the template that produced it, so the template can be deleted.</summary>
        public void ClearRecurringTemplate() => RecurringTransactionId = null;

        public void UpdateAmount(decimal amount)
        {
            ValidateAmount(amount);

            // Lowering the amount below what has already come back would break 0 <= CorrectedAmount <= Amount.
            if (amount < CorrectedAmount)
                throw new InvalidArgumentException(
                    $"Amount cannot be lower than the {CorrectedAmount} already corrected against this transaction.");

            Amount = amount;
        }

        public void UpdateDate(DateOnly occurredOn) => OccurredOn = occurredOn;

        public void Recategorize(int? categoryId) => CategoryId = categoryId;

        public void ChangeType(FinanceTransactionTypeEnum type)
        {
            if (type != Type)
            {
                if (IsCorrection)
                    throw new InvalidArgumentException("A correction inherits its type from the transaction it corrects.");

                // CorrectedAmount > 0 is an exact test for "has corrections" (every correction has Amount > 0),
                // so this holds without the Corrections collection being loaded.
                if (CorrectedAmount > 0)
                    throw new InvalidArgumentException("Type cannot be changed while corrections exist for this transaction.");
            }

            Type = type;
        }

        /// <summary>Registers a correction of <paramref name="amount"/> against this transaction.</summary>
        public void ApplyCorrection(decimal amount)
        {
            ValidateAmount(amount);

            if (IsCorrection)
                throw new InvalidArgumentException("A correction cannot itself be corrected.");

            var corrected = CorrectedAmount + amount;
            if (corrected > Amount)
                throw new InvalidArgumentException(
                    $"Corrections cannot exceed the transaction amount ({Amount}); {Amount - CorrectedAmount} remains correctable.");

            CorrectedAmount = corrected;
        }

        /// <summary>Removes a previously registered correction of <paramref name="amount"/>.</summary>
        public void RevertCorrection(decimal amount)
        {
            ValidateAmount(amount);

            var corrected = CorrectedAmount - amount;
            if (corrected < 0)
                throw new InvalidArgumentException(
                    $"Cannot revert {amount}; only {CorrectedAmount} is corrected against this transaction.");

            CorrectedAmount = corrected;
        }

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

using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A template that materializes into a real <see cref="FinanceTransaction"/> once a month (rent, tuition,
    /// a subscription). The template is not itself a transaction and never appears in any aggregate.
    /// <para>
    /// <see cref="LastMaterializedOn"/> is a watermark recording how far generation has advanced. It exists
    /// because a materialized transaction is a user-owned record they may well delete: dedupe by existence
    /// alone (the approach <c>Quest.GenerateMissingOccurrences</c> takes, correctly, for system-generated
    /// occurrences) would silently recreate a row the user deliberately removed. The watermark distinguishes
    /// <em>never generated</em> from <em>generated and then deleted</em>.
    /// </para>
    /// </summary>
    public class RecurringTransaction : EntityBase
    {
        public const int MinDayOfMonth = 1;
        public const int MaxDayOfMonth = 31;

        public int Id { get; set; }
        public int UserProfileId { get; private set; }
        public FinanceTransactionTypeEnum Type { get; private set; }
        public int? CategoryId { get; private set; }
        public decimal Amount { get; private set; }
        public string? Note { get; private set; }

        /// <summary>
        /// Target day of the month, 1-31. Days past the end of a short month clamp to its last day
        /// (31 -> 28/29/30), matching the client's existing copy-to-month behaviour.
        /// </summary>
        public int DayOfMonth { get; private set; }

        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// First day of the most recent month generation has covered. Never null in practice: creation stamps
        /// it with the creation month, so a template never materializes the month it was created in.
        /// </summary>
        public DateOnly? LastMaterializedOn { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public FinanceCategory? Category { get; set; }
        public ICollection<FinanceTransaction> Transactions { get; set; } = [];

        protected RecurringTransaction() { }

        private RecurringTransaction(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            int dayOfMonth,
            int? categoryId,
            string? note,
            DateOnly createdInMonth)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidateAmount(amount);
            ValidateDayOfMonth(dayOfMonth);
            ValidateNote(note);

            UserProfileId = userProfileId;
            Type = type;
            Amount = amount;
            DayOfMonth = dayOfMonth;
            CategoryId = categoryId;
            Note = note?.Trim();
            IsActive = true;

            // The creation month counts as already covered — see the class remarks and UpdateSchedule below.
            LastMaterializedOn = FirstOfMonth(createdInMonth);
        }

        /// <summary>
        /// Creates a template. <paramref name="today"/> fixes the watermark, so the first row this template
        /// produces lands in the month *after* creation.
        /// <para>
        /// Deliberate: the client creates a template alongside a real transaction for the current month (the
        /// "repeat monthly" tick on the add-transaction form), and that transaction carries no template link
        /// for the generator to dedupe against. Materializing the creation month would duplicate it every time.
        /// A template that should also cover the current month is served by the caller adding that first
        /// transaction itself.
        /// </para>
        /// </summary>
        public static RecurringTransaction Create(
            int userProfileId,
            FinanceTransactionTypeEnum type,
            decimal amount,
            int dayOfMonth,
            DateOnly today,
            int? categoryId = null,
            string? note = null)
            => new(userProfileId, type, amount, dayOfMonth, categoryId, note, today);

        public void UpdateAmount(decimal amount)
        {
            ValidateAmount(amount);
            Amount = amount;
        }

        public void UpdateNote(string? note)
        {
            ValidateNote(note);
            Note = note?.Trim();
        }

        /// <summary>
        /// Re-points the template at another category, or clears it when <paramref name="categoryId"/> is null.
        /// <para>
        /// Forward-only, on purpose: rows already materialized from this template keep the category they were
        /// created with. Those rows are the user's own records — editable and individually re-categorizable —
        /// and rewriting them would silently restate closed months in analytics and budget progress.
        /// </para>
        /// Ownership and type consistency are the handler's job, as everywhere else in the finance module.
        /// </summary>
        public void UpdateCategory(int? categoryId) => CategoryId = categoryId;

        public void UpdateDayOfMonth(int dayOfMonth)
        {
            ValidateDayOfMonth(dayOfMonth);
            DayOfMonth = dayOfMonth;
        }

        /// <summary>
        /// Pausing and resuming. Resuming re-arms the watermark to the current month rather than leaving it
        /// stale, so a template that sat inactive for three months does not backfill them on resume — a pause
        /// means "don't charge me for these months", not "charge me later".
        /// </summary>
        public void SetActive(bool isActive, DateOnly today)
        {
            if (isActive && !IsActive)
                LastMaterializedOn = FirstOfMonth(today);

            IsActive = isActive;
        }

        /// <summary>Advances the watermark after generation has produced rows up to <paramref name="month"/>.</summary>
        public void MarkMaterialized(DateOnly month)
        {
            var first = FirstOfMonth(month);

            if (LastMaterializedOn is DateOnly current && first <= current)
                return;

            LastMaterializedOn = first;
        }

        private static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

        private static void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");
        }

        private static void ValidateDayOfMonth(int dayOfMonth)
        {
            if (dayOfMonth is < MinDayOfMonth or > MaxDayOfMonth)
                throw new InvalidArgumentException($"DayOfMonth must be between {MinDayOfMonth} and {MaxDayOfMonth}.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > FinanceTransaction.NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {FinanceTransaction.NoteMaxLength} characters.");
        }
    }
}

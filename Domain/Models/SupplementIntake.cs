using Domain.Common;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A dose actually taken — the checkbox. One row per (slot, day), which the database enforces with a
    /// filtered unique index on <c>(ScheduleSlotId, TakenOn)</c>: the same structural guarantee
    /// <c>UNIQUE (QuestId, PeriodStart)</c> gives quest occurrences, applied up front this time rather than
    /// after a three-step repair migration.
    /// <para>
    /// <see cref="TakenOn"/> is a <see cref="DateOnly"/> — "which day did I take it" is a calendar fact and
    /// must not move when the user's timezone changes. <see cref="TakenAt"/> is the UTC instant of the tap.
    /// </para>
    /// <para>
    /// A null <see cref="ScheduleSlotId"/> is a legal ad-hoc intake ("wziąłem dziś jeszcze jedną"). A model
    /// that could only record planned doses would punish the user for the exact deviation worth recording.
    /// A null <see cref="WorkoutSessionId"/> is the norm — the link is set only when the dose was ticked off
    /// inside the training screen, and it is the sole coupling between the two modules.
    /// </para>
    /// </summary>
    public class SupplementIntake : EntityBase
    {
        public int Id { get; set; }

        /// <summary>Denormalized owner, so range queries and ownership checks need no join through the supplement.</summary>
        public int UserProfileId { get; private set; }

        public int SupplementId { get; private set; }
        public int? ScheduleSlotId { get; private set; }
        public DateOnly TakenOn { get; private set; }
        public DateTime TakenAt { get; private set; }
        public decimal Amount { get; private set; }
        public int? WorkoutSessionId { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public Supplement Supplement { get; set; } = null!;
        public SupplementScheduleSlot? ScheduleSlot { get; set; }
        public WorkoutSession? WorkoutSession { get; set; }

        public bool IsAdHoc => ScheduleSlotId is null;

        protected SupplementIntake() { }

        private SupplementIntake(
            int userProfileId,
            int supplementId,
            int? scheduleSlotId,
            DateOnly takenOn,
            DateTime takenAtUtc,
            decimal amount,
            int? workoutSessionId)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");
            if (supplementId <= 0)
                throw new InvalidArgumentException("SupplementId must be greater than zero.");
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");

            UserProfileId = userProfileId;
            SupplementId = supplementId;
            ScheduleSlotId = scheduleSlotId;
            TakenOn = takenOn;
            TakenAt = takenAtUtc;
            Amount = amount;
            WorkoutSessionId = workoutSessionId;
        }

        /// <summary>
        /// Records a dose. When <paramref name="amount"/> is omitted it falls back to the slot's planned
        /// amount, then to the supplement's default — so ticking a checkbox needs no payload beyond the intent.
        /// </summary>
        public static SupplementIntake Create(
            Supplement supplement,
            DateOnly takenOn,
            DateTime takenAtUtc,
            SupplementScheduleSlot? slot = null,
            decimal? amount = null,
            int? workoutSessionId = null)
        {
            ArgumentNullException.ThrowIfNull(supplement);

            if (slot is not null && slot.SupplementId != supplement.Id)
                throw new InvalidArgumentException("Slot does not belong to this supplement.");

            var resolved = amount ?? slot?.Amount ?? supplement.DefaultAmount
                ?? throw new InvalidArgumentException(
                    "Amount is required: the supplement has no default amount and no slot was given.");

            return new SupplementIntake(
                supplement.UserProfileId, supplement.Id, slot?.Id, takenOn, takenAtUtc, resolved, workoutSessionId);
        }

        public void UpdateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");

            Amount = amount;
        }

        public void AttachToSession(int? workoutSessionId) => WorkoutSessionId = workoutSessionId;

        /// <summary>
        /// Clears the slot link so a schedule slot can be deleted without erasing the doses taken against it —
        /// the row simply becomes a historical ad-hoc intake. Same posture as clearing
        /// <c>FinanceTransaction.RecurringTransactionId</c> when its template is deleted.
        /// </summary>
        public void DetachFromSlot() => ScheduleSlotId = null;
    }
}

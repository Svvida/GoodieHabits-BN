using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// One planned dose of a <see cref="Supplement"/> — the "when and how much". Timing is a coarse bucket
    /// because that is how people describe a supplement plan ("rano", "30 min przed treningiem"); an exact
    /// <see cref="TimeOfDay"/> and a workout-relative <see cref="OffsetMinutes"/> are optional refinements.
    /// <para>
    /// v1 slots are <em>daily</em>. Weekday scoping is deferred and purely additive (a satellite table
    /// mirroring <c>WeeklyQuest_Day</c>); <see cref="SupplementTimingEnum.PreWorkout"/> already covers the
    /// "training days only" case, because the in-training panel filters by timing.
    /// </para>
    /// </summary>
    public class SupplementScheduleSlot : EntityBase
    {
        public const int NoteMaxLength = 250;
        public const int MinOffsetMinutes = -720;
        public const int MaxOffsetMinutes = 720;

        public int Id { get; set; }
        public int SupplementId { get; private set; }
        public SupplementTimingEnum Timing { get; private set; }

        /// <summary>Exact clock time, in the user's local calendar. Required when <see cref="Timing"/> is Custom.</summary>
        public TimeOnly? TimeOfDay { get; private set; }

        /// <summary>
        /// Minutes relative to the workout, signed: <c>-30</c> is "30 min przed treningiem". Only meaningful
        /// for <see cref="SupplementTimingEnum.PreWorkout"/> / <see cref="SupplementTimingEnum.PostWorkout"/>.
        /// </summary>
        public int? OffsetMinutes { get; private set; }

        /// <summary>How much to take, in the parent supplement's unit.</summary>
        public decimal Amount { get; private set; }

        public string? Note { get; private set; }

        public Supplement Supplement { get; set; } = null!;
        public ICollection<SupplementIntake> Intakes { get; set; } = [];

        protected SupplementScheduleSlot() { }

        private SupplementScheduleSlot(
            SupplementTimingEnum timing,
            decimal amount,
            TimeOnly? timeOfDay,
            int? offsetMinutes,
            string? note)
        {
            Validate(timing, amount, timeOfDay, offsetMinutes, note);

            Timing = timing;
            Amount = amount;
            TimeOfDay = timeOfDay;
            OffsetMinutes = offsetMinutes;
            Note = note?.Trim();
        }

        public static SupplementScheduleSlot Create(
            SupplementTimingEnum timing,
            decimal amount,
            TimeOnly? timeOfDay = null,
            int? offsetMinutes = null,
            string? note = null)
            => new(timing, amount, timeOfDay, offsetMinutes, note);

        /// <summary>
        /// Stamps the owning supplement. Called by <see cref="Supplement.AddSlot"/> so the parentage is known
        /// before EF's relationship fixup runs — <see cref="SupplementIntake.Create"/> checks it, and a guard
        /// that only works after a round-trip to the database is not a guard.
        /// </summary>
        internal void AttachTo(int supplementId) => SupplementId = supplementId;

        public void Update(
            SupplementTimingEnum timing,
            decimal amount,
            TimeOnly? timeOfDay,
            int? offsetMinutes,
            string? note)
        {
            Validate(timing, amount, timeOfDay, offsetMinutes, note);

            Timing = timing;
            Amount = amount;
            TimeOfDay = timeOfDay;
            OffsetMinutes = offsetMinutes;
            Note = note?.Trim();
        }

        private static void Validate(
            SupplementTimingEnum timing,
            decimal amount,
            TimeOnly? timeOfDay,
            int? offsetMinutes,
            string? note)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");

            if (timing == SupplementTimingEnum.Custom && timeOfDay is null)
                throw new InvalidArgumentException("A custom timing requires a time of day.");

            if (offsetMinutes is int offset && (offset < MinOffsetMinutes || offset > MaxOffsetMinutes))
                throw new InvalidArgumentException($"OffsetMinutes must be between {MinOffsetMinutes} and {MaxOffsetMinutes}.");

            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }
    }
}

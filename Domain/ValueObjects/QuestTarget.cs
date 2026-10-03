using Domain.Enums;
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    /// <summary>
    /// What counts as done within one period — the "how much", separate from the "when"
    /// (<see cref="QuestSchedule"/>). A target of 1 reproduces the old boolean behaviour exactly.
    /// </summary>
    public sealed record QuestTarget
    {
        public const int UnitMaxLength = 20;
        public const decimal MaxAmount = 100_000m;

        /// <summary>How much is required. Whole numbers when <see cref="Unit"/> is null ("2 times").</summary>
        public decimal Amount { get; }

        /// <summary>Null means "times". Otherwise a free label the client renders: "L", "pages", "min".</summary>
        public string? Unit { get; }

        public TargetModeEnum Mode { get; }

        /// <summary>
        /// Caps how many completions one calendar day may contribute, so "2 workouts a week" cannot be
        /// finished twice on a Monday. Meaningless for a Day-unit schedule, where the period *is* a day.
        /// Null means unlimited.
        /// </summary>
        public int? MaxCompletionsPerDay { get; }

        private QuestTarget(decimal amount, string? unit, TargetModeEnum mode, int? maxCompletionsPerDay)
        {
            Amount = amount;
            Unit = unit;
            Mode = mode;
            MaxCompletionsPerDay = maxCompletionsPerDay;
        }

        public static QuestTarget Once() => new(1m, null, TargetModeEnum.AtLeast, null);

        public static QuestTarget Create(
            decimal amount,
            string? unit = null,
            TargetModeEnum mode = TargetModeEnum.AtLeast,
            int? maxCompletionsPerDay = null)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Target amount must be greater than zero.");

            if (amount > MaxAmount)
                throw new InvalidArgumentException($"Target amount cannot exceed {MaxAmount}.");

            unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();

            if (unit is not null && unit.Length > UnitMaxLength)
                throw new InvalidArgumentException($"Target unit cannot exceed {UnitMaxLength} characters.");

            // "2.5 times" is not a thing; "2.5 L" is.
            if (unit is null && decimal.Truncate(amount) != amount)
                throw new InvalidArgumentException("A target counted in times must be a whole number.");

            if (maxCompletionsPerDay is int max && max < 1)
                throw new InvalidArgumentException("MaxCompletionsPerDay must be at least 1.");

            return new QuestTarget(amount, unit, mode, maxCompletionsPerDay);
        }

        /// <summary>True when this target is the plain "tick it once" case the old model could express.</summary>
        public bool IsSingleTick => Amount == 1m && Unit is null && Mode == TargetModeEnum.AtLeast;

        /// <summary>
        /// Scales the target down for a period the quest is only active for part of ("3 times a week"
        /// created on a Saturday asks for 1, not 3). Rounds up so a partial period never asks for nothing,
        /// and never asks for more than the full target.
        /// </summary>
        public decimal ProratedAmount(int activeDays, int fullDays)
        {
            if (activeDays >= fullDays || fullDays <= 0)
                return Amount;

            if (activeDays <= 0)
                return Amount;

            var scaled = Amount * activeDays / fullDays;
            var rounded = Unit is null ? Math.Ceiling(scaled) : Math.Round(scaled, 2, MidpointRounding.AwayFromZero);

            return Math.Clamp(rounded < 0.01m ? 0.01m : rounded, 0.01m, Amount);
        }
    }
}

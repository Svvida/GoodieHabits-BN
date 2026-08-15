using Domain.Enums;

namespace Application.Supplements.Intakes.Dtos
{
    /// <summary>
    /// One row of the daily checklist: a planned dose plus whether it has been ticked off.
    /// <para>
    /// <see cref="PlannedAmount"/> is what the schedule says; <see cref="TakenAmount"/> is what was actually
    /// logged, and the two can differ — editing a slot never rewrites doses already taken against it.
    /// </para>
    /// </summary>
    public class SupplementChecklistItemDto
    {
        public int SupplementId { get; set; }
        public string SupplementName { get; set; } = string.Empty;
        public SupplementUnitEnum Unit { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }

        public int SlotId { get; set; }
        public SupplementTimingEnum Timing { get; set; }
        public TimeOnly? TimeOfDay { get; set; }

        /// <summary>Signed minutes relative to the workout: <c>-30</c> is "30 min przed treningiem".</summary>
        public int? OffsetMinutes { get; set; }

        public decimal PlannedAmount { get; set; }
        public string? Note { get; set; }

        /// <summary>The checkbox.</summary>
        public bool Taken { get; set; }

        public int? IntakeId { get; set; }
        public DateTime? TakenAt { get; set; }
        public decimal? TakenAmount { get; set; }
        public int? WorkoutSessionId { get; set; }
    }
}

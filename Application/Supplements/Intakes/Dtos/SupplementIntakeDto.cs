using Domain.Enums;

namespace Application.Supplements.Intakes.Dtos
{
    /// <summary>
    /// A dose that was actually taken.
    /// <para>
    /// <see cref="ScheduleSlotId"/> is null for an ad-hoc dose — either one taken outside the plan, or one
    /// whose slot has since been deleted. <see cref="WorkoutSessionId"/> is set only when the dose was ticked
    /// off inside the training screen; it is the single link between the supplements and workouts modules.
    /// </para>
    /// </summary>
    public class SupplementIntakeDto
    {
        public int Id { get; set; }
        public int SupplementId { get; set; }
        public string SupplementName { get; set; } = string.Empty;
        public SupplementUnitEnum Unit { get; set; }
        public int? ScheduleSlotId { get; set; }

        /// <summary>Calendar date (<c>"YYYY-MM-DD"</c>) — which day the dose belongs to.</summary>
        public DateOnly TakenOn { get; set; }

        /// <summary>The UTC instant of the tap, unlike <see cref="TakenOn"/>.</summary>
        public DateTime TakenAt { get; set; }

        public decimal Amount { get; set; }
        public int? WorkoutSessionId { get; set; }
    }
}

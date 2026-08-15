using Domain.Enums;

namespace Application.Supplements.Catalog.Dtos
{
    /// <summary>
    /// One planned dose — the "when and how much". A supplement may have several: magnesium morning
    /// <em>and</em> evening is one supplement with two slots, not two supplements.
    /// <para>
    /// The unit is not repeated here; it lives on the supplement, and every slot dose is expressed in it.
    /// </para>
    /// </summary>
    public class SupplementSlotDto
    {
        public int Id { get; set; }
        public SupplementTimingEnum Timing { get; set; }

        /// <summary>Exact local clock time (<c>"HH:mm:ss"</c>), or null for a coarse bucket. Required for <c>Custom</c>.</summary>
        public TimeOnly? TimeOfDay { get; set; }

        /// <summary>Signed minutes relative to the workout: <c>-30</c> is "30 min przed treningiem".</summary>
        public int? OffsetMinutes { get; set; }

        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}

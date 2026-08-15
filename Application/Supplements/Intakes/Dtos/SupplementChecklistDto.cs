namespace Application.Supplements.Intakes.Dtos
{
    /// <summary>
    /// One day's supplement plan and what has been ticked off.
    /// <para>
    /// This single response serves <b>both</b> UI surfaces: the standalone supplements screen renders it
    /// whole, and the in-training panel asks for the same date filtered to
    /// <c>PreWorkout</c>/<c>PostWorkout</c>. That filter is the entire integration between the two modules —
    /// there is no session-scoped supplement resource, deliberately, because the plan has to work on a rest
    /// day too.
    /// </para>
    /// </summary>
    public class SupplementChecklistDto
    {
        public DateOnly Date { get; set; }

        /// <summary>Planned doses from active supplements, ordered by timing.</summary>
        public List<SupplementChecklistItemDto> Items { get; set; } = [];

        /// <summary>
        /// Doses taken outside the plan that day, including ones whose slot has since been deleted. Inactive
        /// supplements can appear here — the plan is gone, what was swallowed is not.
        /// </summary>
        public List<SupplementIntakeDto> AdHoc { get; set; } = [];
    }
}

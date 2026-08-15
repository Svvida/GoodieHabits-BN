using Domain.Enums;

namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// A session without its exercise tree — what the history list renders. Totals are still included, so the
    /// list needs no follow-up call per row.
    /// </summary>
    public class WorkoutSessionSummaryDto
    {
        public int Id { get; set; }

        /// <summary>The template it was started from, or null for an ad-hoc session (or a deleted routine).</summary>
        public int? RoutineId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Calendar date (<c>"YYYY-MM-DD"</c>) — the day the user trained, in their own calendar.</summary>
        public DateOnly PerformedOn { get; set; }

        /// <summary>A real UTC instant, unlike <see cref="PerformedOn"/>.</summary>
        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
        public WorkoutSessionStatusEnum Status { get; set; }
        public string? Note { get; set; }

        /// <summary>Wall-clock length, or null while the session is still running.</summary>
        public int? DurationSeconds { get; set; }

        public WorkoutSessionTotalsDto Totals { get; set; } = new();
    }
}

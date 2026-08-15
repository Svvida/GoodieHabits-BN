using Domain.Enums;

namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// One logged set. Every measurement is nullable — which of them are meaningful is decided by the parent
    /// entry's <c>metricType</c>, and the client renders its inputs from that.
    /// </summary>
    public class WorkoutSetDto
    {
        public int Id { get; set; }
        public int SetNumber { get; set; }
        public int? Reps { get; set; }
        public decimal? Weight { get; set; }
        public int? DurationSeconds { get; set; }
        public decimal? Distance { get; set; }
        public decimal? Rpe { get; set; }
        public WorkoutSetTypeEnum SetType { get; set; }
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Epley estimate for this set, or null when it carries no usable reps/weight pair (and for very high
        /// rep counts, where the formula stops meaning anything). Computed server-side so every client shows
        /// the same number.
        /// </summary>
        public decimal? EstimatedOneRepMax { get; set; }
    }
}

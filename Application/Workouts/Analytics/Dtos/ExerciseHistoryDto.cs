namespace Application.Workouts.Analytics.Dtos
{
    /// <summary>One session's worth of work on a single exercise — a point on the progress chart.</summary>
    public class ExerciseHistoryPointDto
    {
        public int SessionId { get; set; }
        public DateOnly PerformedOn { get; set; }
        public int SetCount { get; set; }
        public int TotalReps { get; set; }
        public decimal TotalVolume { get; set; }

        /// <summary>Heaviest working set that session.</summary>
        public decimal? MaxWeight { get; set; }

        /// <summary>
        /// Best Epley estimate across that session's working sets, or null when none carried a usable
        /// reps/weight pair. Computed here rather than in SQL so the formula's single-rep case stays exact.
        /// </summary>
        public decimal? BestEstimatedOneRepMax { get; set; }
    }

    /// <summary>Progress on one exercise over a date range, oldest first.</summary>
    public class ExerciseHistoryDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public List<ExerciseHistoryPointDto> Points { get; set; } = [];
    }
}

namespace Application.Workouts.Analytics.Dtos
{
    /// <summary>
    /// All-time bests for one exercise, over the user's whole history.
    /// <para>
    /// There is deliberately no estimated one-rep max here — see <c>ExerciseBest</c> for why. Use the
    /// exercise-history endpoint for that number.
    /// </para>
    /// </summary>
    public class PersonalRecordDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public decimal? MaxWeight { get; set; }
        public int? MaxReps { get; set; }
        public decimal? MaxSetVolume { get; set; }
        public int SetCount { get; set; }
        public DateOnly LastPerformedOn { get; set; }
    }
}

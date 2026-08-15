namespace Application.Workouts.Routines.Dtos
{
    /// <summary>
    /// A saved workout template — one runnable session's worth of exercises. This is what the client lists on
    /// "start a session, pick a saved one".
    /// </summary>
    public class WorkoutRoutineDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public List<WorkoutRoutineExerciseDto> Exercises { get; set; } = [];
    }
}

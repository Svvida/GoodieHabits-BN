namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// The full session. Every mutating endpoint in the sessions slice returns this, so a client can repaint
    /// the whole screen from one response instead of stitching partial updates together.
    /// </summary>
    public class WorkoutSessionDto : WorkoutSessionSummaryDto
    {
        public List<WorkoutSessionExerciseDto> Exercises { get; set; } = [];
    }
}

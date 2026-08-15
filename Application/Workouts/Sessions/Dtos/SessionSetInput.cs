using Domain.Enums;

namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// One set as the client sends it.
    /// <para>
    /// Which measurements are <b>required</b> comes from the exercise's metric; extra ones are accepted and
    /// stored (that is how a weighted pull-up gets logged against a reps-only exercise). Set numbering is the
    /// server's job — position in the array is the set number.
    /// </para>
    /// <para>
    /// <see cref="CompletedAt"/> is optional and defaults to now. Send the real instant when replaying a
    /// session logged offline, so the timestamps aren't all bunched at sync time.
    /// </para>
    /// </summary>
    public record SessionSetInput(
        int? Reps = null,
        decimal? Weight = null,
        int? DurationSeconds = null,
        decimal? Distance = null,
        decimal? Rpe = null,
        WorkoutSetTypeEnum SetType = WorkoutSetTypeEnum.Normal,
        DateTime? CompletedAt = null);
}

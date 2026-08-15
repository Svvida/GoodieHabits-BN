namespace Application.Workouts.Settings.Dtos
{
    /// <summary>
    /// Per-user workout settings. <see cref="WeightUnit"/> is the unit every logged weight and every volume
    /// number in this module is read in.
    /// <para>
    /// ⚠️ Changing it does <b>not</b> convert anything already stored — weights are kept exactly as the user
    /// typed them, the same call <c>UserProfile.Currency</c> makes. Switching kg → lb reinterprets history
    /// rather than translating it, which is a deliberate, documented trade rather than a bug.
    /// </para>
    /// </summary>
    public class WorkoutSettingsDto
    {
        public string WeightUnit { get; set; } = string.Empty;
        public List<string> SupportedWeightUnits { get; set; } = [];
    }
}

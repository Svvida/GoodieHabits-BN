namespace Application.Workouts.Settings.Commands.UpdateWeightUnit
{
    // Reinterprets existing weights, never converts them. See WorkoutSettingsDto.
    public record UpdateWeightUnitRequest(string WeightUnit);
}

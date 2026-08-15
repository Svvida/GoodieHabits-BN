using Application.Common.Interfaces;
using Application.Workouts.Settings.Dtos;

namespace Application.Workouts.Settings.Commands.UpdateWeightUnit
{
    public record UpdateWeightUnitCommand(string WeightUnit, int UserProfileId) : ICommand<WorkoutSettingsDto>;
}

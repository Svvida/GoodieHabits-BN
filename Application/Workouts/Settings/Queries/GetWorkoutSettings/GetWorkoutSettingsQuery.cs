using Application.Common.Interfaces;
using Application.Workouts.Settings.Dtos;

namespace Application.Workouts.Settings.Queries.GetWorkoutSettings
{
    public record GetWorkoutSettingsQuery(int UserProfileId) : IQuery<WorkoutSettingsDto>;
}

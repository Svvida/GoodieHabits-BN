using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.RemoveSessionExercise
{
    public record RemoveSessionExerciseCommand(int SessionId, int EntryId, int UserProfileId)
        : ICommand<WorkoutSessionDto>;
}

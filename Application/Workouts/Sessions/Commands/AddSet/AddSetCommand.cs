using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.AddSet
{
    /// <summary>
    /// Logs one set live. The set number is assigned by the server — the client sends what was done, not where
    /// it goes.
    /// </summary>
    public record AddSetCommand(
        int SessionId,
        int EntryId,
        SessionSetInput Set,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}

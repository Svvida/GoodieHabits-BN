using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.UpdateSet
{
    /// <summary>
    /// Corrects an already logged set. Full replacement of its measurements — the set number and its position
    /// are the server's, and stay put.
    /// </summary>
    public record UpdateSetCommand(
        int SessionId,
        int EntryId,
        int SetId,
        SessionSetInput Set,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}

using Application.Common.Interfaces;

namespace Application.Workouts.Sessions.Commands.DeleteSession
{
    public record DeleteSessionCommand(int SessionId, int UserProfileId) : ICommand;
}

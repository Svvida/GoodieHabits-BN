using MediatR;

namespace Application.Common.Notifications
{
    public record UserLeveledUpNotification(int UserProfileId, int NewLevel) : INotification;
}

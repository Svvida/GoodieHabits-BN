using Domain.Models;

namespace Domain.Events.Badges
{
    public record BadgeAwardedEvent(int UserProfileId, Badge Badge);
}

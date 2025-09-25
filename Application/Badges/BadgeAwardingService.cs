using Application.Common;
using Application.Common.Interfaces.Badges;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Badges
{
    public class BadgeAwardingService(IServiceProvider serviceProvider, IPublisher publisher, IUnitOfWork unitOfWork) : IBadgeAwardingService
    {
        public async Task CheckAndAwardBadgesAsync(BadgeTriggerEnum badgeTrigger, UserProfile userProfile, Quest? quest, CancellationToken cancellationToken = default)
        {
            var allBadges = await unitOfWork.Badges.GetAllBadgesAsync(cancellationToken).ConfigureAwait(false);
            // Retrieve all strategies for the specific event type
            var strategies = serviceProvider.GetServices<IBadgeAwardingStrategy>()
                .Where(s => s.Trigger == badgeTrigger);

            // Execute each strategy
            foreach (var strategy in strategies)
                strategy.Apply(userProfile, quest, allBadges);

            // Dispatch badges awarded events
            foreach (var badgeAwardedEvent in userProfile.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(badgeAwardedEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            userProfile.ClearDomainEvents();
        }
    }
}

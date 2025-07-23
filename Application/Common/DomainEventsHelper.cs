using MediatR;

namespace Application.Common
{
    internal static class DomainEventsHelper
    {
        public static INotification CreateDomainEventNotification(object domainEvent)
        {
            var genericType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            return (INotification)Activator.CreateInstance(genericType, domainEvent)!;
        }
    }
}

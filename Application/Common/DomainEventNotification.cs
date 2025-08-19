using MediatR;

namespace Application.Common
{
    public class DomainEventNotification<TDomainEvent> : INotification
    {
        public TDomainEvent DomainEvent { get; }

        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}

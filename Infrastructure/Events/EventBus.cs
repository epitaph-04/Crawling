using MediatR;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Events
{
    public class EventBus : IEventBus
    {
        private readonly IMediator _mediator;

        public EventBus(IMediator mediator)
        {
            _mediator = mediator ?? throw new Exception($"Missing dependency '{nameof(IMediator)}'");
        }

        public virtual async Task Publish(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                await _mediator.Publish(@event);
            }
        }

        public virtual async Task Commit(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                await SendToMessageBroker(@event);
            }
        }

        private async Task SendToMessageBroker(IEvent @event)
        {
            
        }
    }
}
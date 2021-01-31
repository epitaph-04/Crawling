
using MediatR;

namespace Api.Infrastructure.Events
{
    public interface IEventHandler<T> : INotificationHandler<T> where T : IEvent { }
}

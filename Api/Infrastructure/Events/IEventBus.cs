using System.Threading.Tasks;

namespace Api.Infrastructure.Events
{
    public interface IEventBus
    {
        Task Publish(params IEvent[] events);
        Task Commit(params IEvent[] events);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Api.Infrastructure
{
    public interface IQueryBus
    {
        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default(CancellationToken));
    }
}

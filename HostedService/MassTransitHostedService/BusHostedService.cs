using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Util;
using Microsoft.Extensions.Hosting;

namespace HostedService.MassTransitHostedService
{
    /// <summary>
    /// 
    /// </summary>
    public class BusHostedService : IHostedService
    {
        private readonly IBusControl _bus;
        private Task<BusHandle> _startTask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bus"></param>
        public BusHostedService(IBusControl bus)
        {
            _bus = bus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _startTask = _bus.StartAsync(cancellationToken);
            return _startTask.IsCompleted ? _startTask : TaskUtil.Completed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bus.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

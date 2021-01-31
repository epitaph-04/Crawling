using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit.Util;
using MassTransit.Registration;

namespace HostedService.MassTransitHostedService
{
    public class MassTransitHostedService : IHostedService
    {
        private readonly IBusDepot _depot;
        private Task _startTask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="depot"></param>
        public MassTransitHostedService(IBusDepot depot)
        {
            _depot = depot;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _startTask = _depot.Start(cancellationToken);
            return _startTask.IsCompleted ? _startTask : TaskUtil.Completed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _depot.Stop(cancellationToken);
        }
    }
}

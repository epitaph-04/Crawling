using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MassTransit.Monitoring.Health;

namespace HostedService.MassTransitHostedService.HealthCheck
{
    /// <summary>
    /// 
    /// </summary>
    public class BusHealthCheck : IHealthCheck
    {
        private readonly IBusHealth _busHealth;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="healthCheck"></param>
        public BusHealthCheck(IBusHealth healthCheck)
        {
            _busHealth = healthCheck;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            var result = _busHealth.CheckHealth();
            return Task.FromResult(result.Status switch
            {
                BusHealthStatus.Healthy => HealthCheckResult.Healthy(result.Description),
                BusHealthStatus.Degraded => HealthCheckResult.Degraded(result.Description, result.Exception),
                _ => HealthCheckResult.Unhealthy(result.Description, result.Exception)
            });
        }
    }
}

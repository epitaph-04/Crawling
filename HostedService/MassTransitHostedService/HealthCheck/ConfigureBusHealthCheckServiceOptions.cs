using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MassTransit.Monitoring.Health;

namespace HostedService.MassTransitHostedService.HealthCheck
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigureBusHealthCheckServiceOptions : IConfigureOptions<HealthCheckServiceOptions>
    {
        private readonly IEnumerable<IBusHealth> _busHealths;
        private readonly string[] _tags;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="busHealths"></param>
        /// <param name="tags"></param>
        public ConfigureBusHealthCheckServiceOptions(IEnumerable<IBusHealth> busHealths, string[] tags)
        {
            _busHealths = busHealths;
            _tags = tags;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public void Configure(HealthCheckServiceOptions options)
        {
            foreach (var busHealth in _busHealths)
			{
                options.Registrations.Add(new HealthCheckRegistration(busHealth.Name, new BusHealthCheck(busHealth), HealthStatus.Unhealthy, _tags));
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Util;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HostedService.MassTransitHostedService.HealthCheck
{
    /// <summary>
    /// 
    /// </summary>
    public class ReceiveEndpointHealthCheck : IReceiveEndpointObserver, IHealthCheck
    {
        private readonly ConcurrentDictionary<Uri, EndpointStatus> _endpoints;
        /// <summary>
        /// 
        /// </summary>
        public ReceiveEndpointHealthCheck()
        {
            _endpoints = new ConcurrentDictionary<Uri, EndpointStatus>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            KeyValuePair<Uri, EndpointStatus>[] faulted = _endpoints.Where(x => !x.Value.Ready).ToArray();

            HealthCheckResult healthCheckResult;
            if (faulted.Any())
            {
                healthCheckResult = new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"Failed endpoints: {string.Join(",", faulted.Select(x => x.Key))}",
                    faulted.Select(x => x.Value.LastException).FirstOrDefault(e => e != null),
                    new Dictionary<string, object> { ["Endpoints"] = faulted.Select(x => x.Key).ToArray() }
                );
            }
            else
            {
                healthCheckResult = HealthCheckResult.Healthy(
                    "All endpoints ready",
                    new Dictionary<string, object> { ["Endpoints"] = _endpoints.Keys.ToArray() }
                );
            }

            return Task.FromResult(healthCheckResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ready"></param>
        /// <returns></returns>
        public Task Ready(ReceiveEndpointReady ready)
        {
            GetEndpoint(ready.InputAddress).Ready = true;
            return TaskUtil.Completed;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="stopping"></param>
		/// <returns></returns>
		public Task Stopping(ReceiveEndpointStopping stopping) => TaskUtil.Completed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
		public Task Completed(ReceiveEndpointCompleted completed) => TaskUtil.Completed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="faulted"></param>
        /// <returns></returns>
		public Task Faulted(ReceiveEndpointFaulted faulted)
        {
            var endpoint = GetEndpoint(faulted.InputAddress);
            endpoint.Ready = false;
            endpoint.LastException = faulted.Exception;
            return TaskUtil.Completed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputAddress"></param>
        /// <returns></returns>
        EndpointStatus GetEndpoint(Uri inputAddress)
        {
            if (!_endpoints.ContainsKey(inputAddress))
                _endpoints.TryAdd(inputAddress, new EndpointStatus());

            return _endpoints[inputAddress];
        }

        class EndpointStatus
        {
            public bool Ready { get; set; }
            public Exception LastException { get; set; }
        }
    }
}

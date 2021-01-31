using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly MemoryCheckOptions _option;

        public MemoryHealthCheck(MemoryCheckOptions option)
        {
            _option = option;
        }

        public string Name => "memory_check";

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Include GC information in the reported diagnostics.
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var data = new Dictionary<string, object>()
            {
                { "AllocatedBytes", allocated },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) },
            };
            var status = (allocated < _option.Threshold) ? HealthStatus.Healthy : context.Registration.FailureStatus;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: "Reports degraded status if allocated bytes " + $">= {_option.Threshold} bytes.",
                exception: null,
                data: data)
            );
        }
    }

    public class MemoryCheckOptions
    {
		public long Threshold { get; set; }
	}
}

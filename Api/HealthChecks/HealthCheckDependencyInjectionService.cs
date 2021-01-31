using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Api.HealthChecks
{
	public static class HealthCheckDependencyInjectionService
	{
        public static IHealthChecksBuilder AddMemoryHealthCheck(this IHealthChecksBuilder builder,  string name, Action<MemoryCheckOptions> setup, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new MemoryCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? "memory",
                sp => new MemoryHealthCheck(options),
                failureStatus,
                tags,
                timeout)
            );
        }
    }
}

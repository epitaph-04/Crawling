using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HostedService.CronJobScheduler;
using LinkFetcher;

namespace LiveTvWorker
{
    public class Worker : CronJobService
	{
        private readonly ILogger<Worker> _logger;

		public Worker(IScheduleConfig<Worker> config, ILogger<Worker> logger)
			: base(config.CronExpression, config.TimeZoneInfo)
		{
            _logger = logger;
        }

		public override async Task DoWork(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
			await new LiveTv().Fetch();
		}
	}
}

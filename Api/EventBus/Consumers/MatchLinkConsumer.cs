using LinkFetcher;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.EventBus.Consumers
{
	/// <summary/>
	public class MatchLinkConsumer : IConsumer<IEnumerable<LinkModel>>
	{
		ILogger<MatchLinkConsumer> _logger;
		/// <summary/>
		public MatchLinkConsumer(ILogger<MatchLinkConsumer> logger)
		{
			_logger = logger;
		}
		/// <summary/>
		public async Task Consume(ConsumeContext<IEnumerable<LinkModel>> context)
		{
			await context.ConsumeCompleted;
		}
	}
}
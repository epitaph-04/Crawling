using Api.Infrastructure;
using LinkFetcher;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.CronJob
{
    public class LinkFetcherJob : CronJobService
    {
        private readonly ILogger<LinkFetcherJob> _logger;
		private List<LinkModel> linkModels;
		private readonly RemoteDriverProperties remoteDriverProperties;

		public LinkFetcherJob(IScheduleConfig<LinkFetcherJob> config, IOptions<RemoteDriverProperties> options, ILogger<LinkFetcherJob> logger)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
			remoteDriverProperties = options.Value;

		}
        public override async Task DoWork(CancellationToken cancellationToken)
        {
			linkModels = new List<LinkModel>();
			SemaphoreSlim ss = new SemaphoreSlim(1);
			var liveTvLinkTask = new LiveTv().Fetch().ContinueWith(async t => await UpdateModel(ss, t.Result.ToList()));
			//var sport365LinkTask = new Sport365(remoteDriverProperties.RemoteUrl).Fetch().ContinueWith(async t => await UpdateModel(ss, t.Result.ToList()));
			await Task.WhenAll(liveTvLinkTask);
			LiveLink.Current = new LiveLink { Link = linkModels };
		}
		private async Task UpdateModel(SemaphoreSlim ss, List<LinkModel> items)
		{
			await ss.WaitAsync();
			try
			{
				if (items.Any())
				{
					foreach (var link in items)
					{
						var model = linkModels.FirstOrDefault(x => x.LinkInfo == link.LinkInfo);
						if (model != null) linkModels.Remove(model);
						var links = new HashSet<string>(link.Links.Concat(model?.Links ?? Enumerable.Empty<string>()));
						model = new LinkModel(link.LinkInfo, links);
						linkModels.Add(model);
					}
				}
				else
				{
					linkModels.AddRange(items);
				}
			}
			finally
			{
				ss.Release();
			}
		}
	}
}


using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LinkFetcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SportApi.Extensions;
using SportApi.Scheduler;

namespace SportApi
{
	public class LinkFetcherTask : IScheduledTask
	{
		public string Schedule => "*/5 * * * *";
		private List<LinkModel> linkModels;

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			linkModels = new List<LinkModel>();
			SemaphoreSlim ss = new SemaphoreSlim(1);
			var liveTvLinkTask = new LiveTv().Fetch().ContinueWith(async t => await UpdateModel(ss, t.Result.ToList()));
			var sport365LinkTask = new Sport365().Fetch().ContinueWith(async t => await UpdateModel(ss, t.Result.ToList()));
			await Task.WhenAll(liveTvLinkTask, sport365LinkTask);
			await GenerateImages();
			LiveLink.Current = new LiveLink { Link = linkModels };
		}

		private async Task GenerateImages()
		{
			var executableLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var imagePath = Path.Combine(executableLocation, "resources");
			var ffmpegPath = Path.Combine(executableLocation, "ffmpeg/ffmpeg.exe");

			DirectoryInfo di = new DirectoryInfo(imagePath);
			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
			foreach (var linkmodel in linkModels.GroupBy(x => x.LinkInfo.Title))
			{
				var output = Path.Combine(imagePath, $"{linkmodel.Key}.jpg");
				var path = linkmodel.First()?.Links.FirstOrDefault();
				var arguments = $"-ss 00:00:01 -i \"{path}\" -frames 1 \"{output}\"";
				await ProcessExtensions.RunProcessAsync(ffmpegPath, arguments);
			}
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

	public class LiveLink
	{
		public static LiveLink Current { get; set; }

		static LiveLink()
		{
			Current = new LiveLink { Link = Enumerable.Empty<LinkModel>() };
		}

		public IEnumerable<LinkModel> Link { get; set; }
	}
}
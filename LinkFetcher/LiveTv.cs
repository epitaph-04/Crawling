using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System;

namespace LinkFetcher
{
	public class LiveTv : ILiveTvLink
	{
		private string rootUrl = "http://livetv.sx";
		private readonly HttpClient client;

		public LiveTv()
		{
			HttpClientHandler handler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};
			client = new HttpClient(handler);
		}

		public async Task<IEnumerable<LinkModel>> Fetch()
		{
			var m3u8Links = new HashSet<Tuple<LinkInfo, IEnumerable<string>>>();
			var liveLinks = new HashSet<Tuple<LinkInfo, IEnumerable<string>>>();
			var links = await FetchRoot();
			foreach (var link in links)
			{
				liveLinks.Add(await FetchLiveLink(link));
			}
			foreach (var link in liveLinks)
			{
				m3u8Links.Add(await FetchM3U8Link(link));
			}
			return m3u8Links.Where(x => x.Item2.Any()).Select(x => new LinkModel(x.Item1, x.Item2)).ToList();
		}

		private async Task<IEnumerable<string>> FetchRoot()
		{
			var Links = new HashSet<string>();
			using (var response = await client.GetAsync(rootUrl + "/enx/livescore/"))
			{
				using (var content = response.Content)
				{
					var result = await content.ReadAsStringAsync();
					var document = new HtmlDocument();
					document.LoadHtml(result);
					var nodes = document.DocumentNode.Descendants("a");
					foreach (var node in nodes)
					{
						Links.Add(ParseHTML(node));
					}
				}
			}
			return Links;
		}

		private async Task<Tuple<LinkInfo, IEnumerable<string>>> FetchLiveLink(string link)
		{
			if (string.IsNullOrEmpty(link)) return new Tuple<LinkInfo, IEnumerable<string>>(new LinkInfo(), Enumerable.Empty<string>());
			var playerLink = new HashSet<string>();
			var title = string.Empty;
			var time = string.Empty;
			var playerLinkUrl = "cdn.livetvcdn.net/webplayer.php";
			using (var response = await client.GetAsync(rootUrl + link))
			{
				using (var content = response.Content)
				{
					var result = await content.ReadAsStringAsync();
					var document = new HtmlDocument();
					document.LoadHtml(result);
					title = document.DocumentNode.SelectSingleNode("//h1[@itemprop='name']")?.Descendants("b")?.FirstOrDefault()?.InnerText;
					time = GetTime(document.DocumentNode.SelectSingleNode("//meta[@itemprop='startDate']")?.GetAttributeValue("content", "Not found"));
					if (title == null) return new Tuple<LinkInfo, IEnumerable<string>>(new LinkInfo(), Enumerable.Empty<string>());
					var nodes = document.DocumentNode.Descendants("a").Where(a => a.GetAttributeValue("href", "Not found").Contains(playerLinkUrl));
					foreach (var node in nodes)
					{
						playerLink.Add(node.GetAttributeValue("href", "Not found"));
					}
				}
			}

			return new Tuple<LinkInfo, IEnumerable<string>>(new LinkInfo { Title = title.Replace("&ndash;", "vs"), Time = time }, playerLink);
		}

		private async Task<Tuple<LinkInfo, IEnumerable<string>>> FetchM3U8Link(Tuple<LinkInfo, IEnumerable<string>> liveLink)
		{
			var m3u8Link = new HashSet<string>();
			foreach (var link in liveLink.Item2)
			{
				var httpLink = "http:" + link;
				using (var response = await client.GetAsync(httpLink))
				{
					using (var content = response.Content)
					{
						var result = await content.ReadAsStringAsync();
						var document = new HtmlDocument();
						document.LoadHtml(result);
						var nodes = document.DocumentNode.Descendants("iframe");

						ParseAndGetM3u8Link(nodes, m3u8Link);
					}
				}
			}
			return new Tuple<LinkInfo, IEnumerable<string>>(liveLink.Item1, m3u8Link.Where(x => !string.IsNullOrEmpty(x)));
		}

		private void ParseAndGetM3u8Link(IEnumerable<HtmlNode> nodes, HashSet<string> m3u8Link)
		{
			var iframes = new List<HtmlNode>();
			var web = new HtmlWeb();
			foreach (var node in nodes)
			{
				try
				{
					var iframeDocument = web.Load(node.GetAttributeValue("src", "not found"));
					foreach (var script in iframeDocument.DocumentNode.Descendants("script"))
					{
						m3u8Link.Add(ParseM3U8Url(script.InnerText));
					}
					foreach (var video in iframeDocument.DocumentNode.Descendants("video"))
					{
						m3u8Link.Add(ParseM3U8Url(video.InnerText));
					}
					iframes.AddRange(iframeDocument.DocumentNode.Descendants("iframe"));
				}
				catch { }
			}
			if(iframes.Any())
				ParseAndGetM3u8Link(iframes, m3u8Link);
		}

		private string ParseM3U8Url(string script)
		{
			var regex = new Regex(@"(http(s)?://)([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?");
			var match = regex.Match(script);
			return match.Success ? (match.Value.Contains(".m3u8") ? match.Value : string.Empty) : string.Empty;
		}
		private string ParseHTML(HtmlNode node)
		{
			if (node.InnerText.Equals("Video")) return node.GetAttributeValue("href", "not found");
			return string.Empty;
		}

		private string GetTime(string text)
		{
			var timeformat = "(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]";
			Regex regex = new Regex(timeformat);
			return regex.Match(text).Value;
		}
	}
}

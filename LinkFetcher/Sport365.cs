using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinkFetcher.Extensions;
using OpenQA.Selenium.Chrome;
using System.IO;
using System;
using OpenQA.Selenium.Support.UI;

namespace LinkFetcher
{
	public class Sport365 : ILiveTvLink
	{
		private string assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
		private string rootUrl = "http://www.sport365.live/en/home";
		private readonly HttpClient client;
		private ChromeDriver driver;

		public Sport365()
		{
			HttpClientHandler handler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};
			client = new HttpClient(handler);

			
		}

		public async Task<IEnumerable<LinkModel>> Fetch()
		{
			ChromeOptions option = new ChromeOptions();
			option.AddArgument("--headless");
			driver = new ChromeDriver(assemblyPath, option);

			var matchLinks = new List<LinkModel>();
			try
			{
				var liveLinks = new HashSet<string>();
				var links = await FetchLiveLink();

				foreach (var link in links)
				{
					var m3u8Links = new HashSet<string>();
					foreach (var httpLink in link.Value)
						m3u8Links.Add(await FetchM3U8Link(httpLink));
					matchLinks.Add(new LinkModel(link.Key, m3u8Links));
				}
			}
			finally
			{
				driver.Close();
				driver.Dispose();
			}
			
			return matchLinks;
		}

		private async Task<Dictionary<LinkInfo, HashSet<string>>> FetchLiveLink()
		{
			var Links = new Dictionary<LinkInfo, HashSet<string>>();
			driver.Navigate().GoToUrl(rootUrl);
			driver.FindElement(OpenQA.Selenium.By.XPath("//img[@alt='Watch now']")).Click();
			await Task.Delay(1000);
			var links = driver.FindElements(OpenQA.Selenium.By.XPath("//div[@id='content-right']//div[@id='content_div']//div[@class='post']//div[@id='events']//table//tbody/tr[@title='Live']"));

			var oddEvenLinks = links.Select((item, index) => new { Item = item, Index = index })
					 .GroupBy(x => x.Index % 2 == 0)
					 .ToDictionary(g => g.Key, g => g);
			try
			{
				var document = new HtmlDocument();
				foreach (var element in oddEvenLinks[true].Select(x => x.Item))
				{
					if (element.Displayed)
					{

						var linkInfo = GetLinkInfo(element.Text);
						if(!Links.TryGetValue(linkInfo, out var httpLinks)) httpLinks = new HashSet<string>();

						element.Click();
						await Task.Delay(500);
						var codeLinks = driver.FindElements(OpenQA.Selenium.By.XPath("//div[@id='link_list']//table//tbody//tr//td[@class='row1']//span[@id='span_code_links']"));
						foreach (var code in codeLinks)
						{
							if (code.Displayed)
							{
								code.Click();
								await Task.Delay(500);

								var popupArea = driver.FindElements(OpenQA.Selenium.By.XPath("//textarea[@id='popup-code-control']")).FirstOrDefault();
								if (popupArea != null)
								{
									httpLinks.Add(ParseUrl(popupArea.Text, "/", new[] { "1024", "724" }.ToList()));
									driver.FindElement(OpenQA.Selenium.By.XPath("//div[@id='popup-box']//table/tbody/tr/td/img")).Click();
									await Task.Delay(1000);
								}

							}
						}
						element.Click();
						await Task.Delay(500);
						Links[linkInfo] = httpLinks;
					}
				}
			}
			catch { }
			return Links;
		}

		private async Task<string> FetchM3U8Link(string liveLink)
		{
			var m3u8Link = new HashSet<string>();

			driver.Navigate().GoToUrl(liveLink);
			await Task.Delay(1000);
			var iframe = driver.FindElement(OpenQA.Selenium.By.XPath("//div[@id='area-middle']//iframe"));

			driver.SwitchTo().Frame(driver.FindElement(OpenQA.Selenium.By.XPath("//div[@id='area-middle']//iframe")));
			driver.SwitchTo().Frame(driver.FindElement(OpenQA.Selenium.By.XPath("//iframe")));

			var document = new HtmlDocument();
			document.LoadHtml(driver.PageSource);
			var source = document.DocumentNode.SelectSingleNode("//source").GetAttributeValue("src", "not found");
			return string.Join(".", new[] { source, "m3u8" });
		}

		private string ParseUrl(string script, string separator, List<string> arguments)
		{
			var regex = new Regex(@"(http(s)?://)([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?");
			var match = regex.Match(script);
			arguments.Insert(0, match.Value);
			return match.Success ? string.Join(separator, arguments) : string.Empty;
		}

		private LinkInfo GetLinkInfo(string text)
		{
			var timeformat = "(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]";
			var live = @"( \(Live\) )";
			var hq = " ";
			var dashReplace = " - ";
			Regex regex = new Regex(timeformat);
			var time = regex.Match(text).Value;

			text = regex.Replace(text, "");
			regex = new Regex(live);
			var titleInfos = regex.Split(text);

			regex = new Regex(hq);
			var languageInfos = regex.Split(titleInfos[2].Trim());

			regex = new Regex(dashReplace);
			return new LinkInfo
			{
				Title = regex.Replace(titleInfos[0], " vs ").Trim(),
				Time = time.Trim(),
				Language = languageInfos.Count() > 1 ? languageInfos[1] : languageInfos[0],
				Quality = languageInfos.Count() > 1 ?(VideoQuality) Enum.Parse(typeof(VideoQuality), languageInfos[0]) : VideoQuality.MQ
			};
		}
	}
}
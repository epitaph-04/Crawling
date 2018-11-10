using System;
using System.Collections.Generic;
using System.Text;

namespace LinkFetcher
{
	public class LinkModel
	{
		public LinkModel(LinkInfo linkInfo, IEnumerable<string> links)
		{
			LinkInfo = linkInfo;
			Links = links;
		}
		public LinkInfo LinkInfo { get; }
		public IEnumerable<string> Links { get; }
	}

	public class LinkInfo : IEquatable<LinkInfo>
	{
		public string Title { get; set; }
		public string Time { get; set; }
		public string Language { get; set; }
		public VideoQuality Quality { get; set; }

		public bool Equals(LinkInfo other)
		{
			return Title.Equals(other.Title) && Time.Equals(other.Time) && Language.Equals(other.Language) && Quality.Equals(other.Quality);
		}
	}

	public enum VideoQuality { MQ, HQ }
}

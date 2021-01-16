using LinkFetcher;
using System.Collections.Generic;
using System.Linq;

namespace Api
{
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

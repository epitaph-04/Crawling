using LinkFetcher;
using System.Collections.Generic;

namespace Api.Service
{
	public class LinkRepository<T> : ILinkRepository<T> where T : class
	{
		private readonly List<T> links;
		public LinkRepository()
		{
			links = new List<T>();
		}
		public IEnumerable<T> GetLinks() => links;

		public void SetLinks(IEnumerable<T> links)
		{
			this.links.Clear();
			this.links.AddRange(links);
		}
	}
}

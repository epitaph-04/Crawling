using System.Collections.Generic;

namespace Api.Service
{
	interface ILinkRepository<T> where T : class
	{
		void SetLinks(IEnumerable<T> links);
		IEnumerable<T> GetLinks();
	}
}

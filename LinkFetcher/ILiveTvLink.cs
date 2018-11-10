using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkFetcher
{
	public interface ILiveTvLink
	{
		Task<IEnumerable<LinkModel>> Fetch();
	}
}

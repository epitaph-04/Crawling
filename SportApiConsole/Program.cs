using System;
using System.Threading.Tasks;
using LinkFetcher;

namespace SportApiConsole
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			var link = await new Sport365().Fetch();
		}
	}
}

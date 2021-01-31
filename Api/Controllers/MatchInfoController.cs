using Api.Infrastructure;
using LinkFetcher;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MatchInfoController : ControllerBase
	{
		private readonly IQueryBus _queryBus;

		public MatchInfoController(IQueryBus queryBus)
		{
			_queryBus = queryBus;
		}

		[HttpGet]
		public async Task<IEnumerable<LinkModel>> GetAsync()
		{
			var query = new GetLinkQuery();
			var result = await _queryBus.Send(query, CancellationToken.None);

			return Ok(result);
		}
	}
}

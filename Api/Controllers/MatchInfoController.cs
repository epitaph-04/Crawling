using LinkFetcher;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MatchInfoController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<LinkModel> Get() => LiveLink.Current.Link;
	}
}

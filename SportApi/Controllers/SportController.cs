using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinkFetcher;

namespace SportApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SportController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<LinkModel> Get()
		{
			return LiveLink.Current.Link;
		}
	}
}
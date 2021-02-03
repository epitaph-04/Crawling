using System.Collections.Generic;
using Infrastructure;
using LinkFetcher;

namespace Api.Query
{
    internal class GetLinkQuery : IQuery<IEnumerable<LinkModel>>
    {
        public GetLinkQuery()
        {
        }
    }
}
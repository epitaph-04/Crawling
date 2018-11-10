using System.Collections.Generic;

namespace LinkFetcher.Extensions
{
	public static class ICollectionExtension
	{
		public static void AddRange<T>(this ICollection<T> sourceCollection, IEnumerable<T> items)
		{
			foreach(var item in items)
			{
				sourceCollection.Add(item);
			}
		}
	}
}
using MassTransit.Topology;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Api.EventBus
{
	/// <summary>
	/// 
	/// </summary>
	public class MessageEntityNameFormatter : IEntityNameFormatter
	{
		Dictionary<string, string> messageTypeMapper;

		/// <summary/>
		/// <param name="options"></param>
		public MessageEntityNameFormatter(IOptions<ServiceBrokerConfigurator> options)
		{
			messageTypeMapper = options.Value.MessageExchange;
		}

		/// <summary/>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public string FormatEntityName<T>()
		{
			var typeName = typeof(T).Name;
			return messageTypeMapper.ContainsKey(typeName) ?  messageTypeMapper[typeName] : $"events.live-match.{typeName}.e";
		}
	}
}
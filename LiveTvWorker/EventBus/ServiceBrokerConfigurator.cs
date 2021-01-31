using System.Collections.Generic;

namespace LiveTvWorker
{
	/// <summary/>
	public class ServiceBrokerConfigurator
	{
		/// <summary/>
		public string Host { get; set; }
		/// <summary/>
		public string VirtualHost { get; set; }
		/// <summary/>
		public string Username { get; set; }
		/// <summary/>
		public string Password { get; set; }
		/// <summary/>
		public Dictionary<string, string> MessageExchange { get; set; }
	}
}

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SportApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().RunAsMyService();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			var configuration = new ConfigurationBuilder().AddCommandLine(args).Build();
			
			return WebHost.CreateDefaultBuilder(args)
						  .UseKestrel()
						  .UseUrls("http://localhost:60000", "http://0.0.0.0:60000")
						  .UseIISIntegration()
						  .UseConfiguration(configuration)
						  .UseStartup<Startup>();
		}
	}
}
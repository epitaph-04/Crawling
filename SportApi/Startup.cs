using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using SportApi.Scheduler;

namespace SportApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc()
				.AddJsonOptions(options => options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true }))
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			// Add scheduled tasks & scheduler
			services.AddSingleton<IScheduledTask, LinkFetcherTask>();
			services.AddScheduler((sender, args) =>
			{
				args.SetObserved();
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
		}
	}
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Converters;
using SportApi.Scheduler;
using System.IO;

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
			var executableLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var imagePath = new DirectoryInfo(Path.Combine(executableLocation, "resources")).FullName;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(imagePath),
				RequestPath = "/StaticFiles"
			});
			app.UseMvc();
		}
	}
}
using Api.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Api.Service;
using LinkFetcher;
using MassTransit;
using Microsoft.Extensions.Options;
using MassTransit.Topology;
using Api.EventBus;

namespace Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<ILinkRepository<LinkModel>, LinkRepository<LinkModel>>();

			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" }))
					.AddControllers();

			services.AddMassTransit(x =>
			{
				x.AddConsumersFromNamespaceContaining<Startup>();
				x.UsingRabbitMq((context, cfg) =>
				{
					var brokerConfiguration = context.GetRequiredService<IOptions<ServiceBrokerConfigurator>>();
					cfg.Host(brokerConfiguration.Value.Host, brokerConfiguration.Value.VirtualHost, h =>
					{
						h.Username(brokerConfiguration.Value.Username);
						h.Password(brokerConfiguration.Value.Password);
					});
					cfg.MessageTopology.SetEntityNameFormatter(context.GetRequiredService<IEntityNameFormatter>());
					cfg.ReceiveEndpoint($"events.api.subscription.e-{NewId.NextGuid()}", e =>
					{
						e.Durable = false;
						e.AutoDelete = true;
						e.AutoStart = false;
						e.ConfigureConsumers(context);
					});
				});
			});
			services.AddMassTransitHostedService();

			services.AddHealthChecks()
					.AddRabbitMQ(rabbitConnectionString: "amqp://guest:guest@localhost:5672/")
					.AddMemoryHealthCheck("memory", option => option.Threshold = 1024 * 1024 * 128, HealthStatus.Degraded, new[] { "memory" });
			
			services.AddHealthChecksUI()
					.AddInMemoryStorage();


		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger(setup => setup.RouteTemplate = "api/{documentName}/swagger.json");
				app.UseSwaggerUI(c => 
				{ 
					c.RoutePrefix = "api";
					c.SwaggerEndpoint("/api/v1/swagger.json", "Api v1"); 
				});
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/health", new HealthCheckOptions
				{
					Predicate = _ => true,
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});
				endpoints.MapHealthChecksUI(s => { s.UIPath = "/hc-ui"; s.ApiPath = "/hc-api"; }) ;
				endpoints.MapControllers();
			});
		}
	}
}

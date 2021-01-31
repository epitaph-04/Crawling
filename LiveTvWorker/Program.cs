using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using MassTransit.Topology;
using HostedService.CronJobScheduler;
using HostedService.MassTransitHostedService;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LiveTvWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
				.ConfigureHostConfiguration(configure =>
				{
					configure.AddJsonFile("eventsource.json", optional: false);
					configure.AddJsonFile("eventsource.Production.json", optional: true);
				})
                .ConfigureServices((hostContext, services) =>
                {
					services.AddCronJob<Worker>(c =>
                    {
                        c.TimeZoneInfo = TimeZoneInfo.Local;
                        c.CronExpression = @"*/5 * * * *";
                    });
					services.Configure<ServiceBrokerConfigurator>(hostContext.Configuration.GetSection("eventSubscription:rabbitmqSettings"));
					services.AddTransient<IEntityNameFormatter, MessageEntityNameFormatter>();

					services.AddMassTransit(x =>
					{
						x.UsingRabbitMq((context, cfg) =>
						{
							var brokerConfiguration = context.GetRequiredService<IOptions<ServiceBrokerConfigurator>>();
							cfg.Host(brokerConfiguration.Value.Host, brokerConfiguration.Value.VirtualHost, h =>
							{
								h.Username(brokerConfiguration.Value.Username);
								h.Password(brokerConfiguration.Value.Password);
							});
							cfg.MessageTopology.SetEntityNameFormatter(context.GetRequiredService<IEntityNameFormatter>());
						});
					});
					services.AddMassTransitHostedService();
				});
    }
}

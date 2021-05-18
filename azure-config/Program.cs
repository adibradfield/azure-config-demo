using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace azure_config
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureAppConfiguration((context, builder) =>
                    {
                        if(!context.HostingEnvironment.IsDevelopment()){
                            var tempConfig = builder.Build();

                            builder.AddAzureAppConfiguration(opt =>
                            {
                                opt.Connect(tempConfig["ConnectionStrings:AppConfiguration"]);
                                opt.UseFeatureFlags(flags => {
                                    flags.Select("*");
                                    flags.Select("*", context.HostingEnvironment.EnvironmentName);
                                    //flags.CacheExpirationInterval = TimeSpan.FromSeconds(2);
                                    flags.CacheExpirationInterval = TimeSpan.FromDays(1);
                                });

                                opt.Select("*");
                                opt.Select("*", context.HostingEnvironment.EnvironmentName);

                                opt.ConfigureRefresh(refresh =>
                                {
                                    refresh.Register("Sentinel", refreshAll: true);
                                    //refresh.SetCacheExpiration(TimeSpan.FromSeconds(2));
                                    refresh.SetCacheExpiration(TimeSpan.FromDays(1));
                                });

                                SetupConfigurationRefresh(tempConfig, opt.GetRefresher());
                            }, optional: false);
                        }
                    });
                });

        private static void SetupConfigurationRefresh(IConfiguration configuration, IConfigurationRefresher refresher)
        {
            var bus = new SubscriptionClient(configuration["ConnectionStrings:ServiceBus"], configuration["ServiceBus:Topic"], configuration["ServiceBus:SubscriptionName"]);
            bus.RegisterMessageHandler(
                handler: (message, cancellationToken) =>
                {
                    string messageText = Encoding.UTF8.GetString(message.Body);
                    JsonElement messageData = JsonDocument.Parse(messageText).RootElement.GetProperty("data");
                    string key = messageData.GetProperty("key").GetString();
                    Console.WriteLine($"Configuration refresh event received for Key = {key}");

                    refresher.SetDirty();
                    return Task.CompletedTask;
                },
                exceptionReceivedHandler: (exceptionargs) =>
                {
                    Console.WriteLine($"{exceptionargs.Exception}");
                    return Task.CompletedTask;
                });
        }
    }
}

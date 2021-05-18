// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Data.AppConfiguration;
using System.Threading.Tasks;

namespace Company.Function
{
    public static class AzureConfigUpdater
    {
        [FunctionName("AzureConfigUpdater")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            dynamic data = eventGridEvent.Data;

            if(data.key != "Sentinel"){
                var client = new ConfigurationClient(System.Environment.GetEnvironmentVariable("AppConfigurationConnectionString", EnvironmentVariableTarget.Process));
                await client.SetConfigurationSettingAsync("Sentinel", Guid.NewGuid().ToString());
            }
        }
    }
}

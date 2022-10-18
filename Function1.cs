using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;

namespace AZEventGridReceiver
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string response = string.Empty;

            try
            {
                log.LogInformation($"HTTP Trigger function for event grid learnings");

                BinaryData events = await BinaryData.FromStreamAsync(req.Body);
                log.LogInformation($"Events {events}");

                EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);

                foreach (EventGridEvent eventGridEvent in eventGridEvents)
                {
                    if (eventGridEvent.TryGetSystemEventData(out object eventData))
                    {
                        if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                        {
                            log.LogInformation($"Got Subscription validation event data | validation code {subscriptionValidationEventData.ValidationCode} | Topic {eventGridEvent.Topic}");

                            var responseData = new SubscriptionValidationResponse()
                            {
                                ValidationResponse = subscriptionValidationEventData.ValidationCode
                            };
                            return new OkObjectResult(responseData);
                        }
                    }
                    else if (eventGridEvent.EventType == "Learning.EventGrid.Test")
                    {
                        log.LogInformation($"EventType Learning.EventGrid.Test");

                        var learningEventData = eventGridEvent.Data.ToObjectFromJson<string>();

                        log.LogInformation($"Got learningeventData {learningEventData}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"exception {ex.ToString()}");
            }

            return new OkObjectResult(response);
        }
    }
}

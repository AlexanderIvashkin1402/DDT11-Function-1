using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System;

namespace Company.Function
{
    public static class HttpTrigger1
    {
        static string connectionString = "Endpoint=sb://ddt11sb-1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Y3wzgaRrO+98IiV6lHS+f7SzUP1sVkSdY+ASbAJeAAg=";
        static string topicName = "oddeven";
        static ServiceBusClient client;
        static ServiceBusSender sender;
        
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(topicName);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            if(!messageBatch.TryAddMessage(new ServiceBusMessage(requestBody)))
            {
                throw new Exception("The message is too large to fit the batch");
            }      
               
            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine("A batch of messages has been published to the topic");
            }
            finally
            {   
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new OkObjectResult("Payload Received");
        }
    }
}

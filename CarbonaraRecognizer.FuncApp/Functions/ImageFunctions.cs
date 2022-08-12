#define FUNCTIONSTANDARD

using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.Core.Interfaces;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.Functions
{
    public class ImageFunctions
    {
        private readonly IImageAnalyzer imageAnalyzer;
        private readonly IConfiguration configuration;

        public ImageFunctions(IImageAnalyzer imageAnalyzer, IConfiguration configuration)
        {
            this.imageAnalyzer = imageAnalyzer;
            this.configuration = configuration;
        }

#if FUNCTIONSTANDARD
        [FunctionName("ImageUploaded")]
        public async Task Run(
            [BlobTrigger("%SourceContainer%/{name}", Connection = "SourceStorageConnectionString")] ICloudBlob inputImageClient,
            string name,
            [Blob("%DestinationContainer%/{name}", FileAccess.ReadWrite, Connection = "DestinationStorageConnectionString")] ICloudBlob validImageClient,
            [Blob("%TrashbinContainer%/{name}", FileAccess.ReadWrite, Connection = "DestinationStorageConnectionString")] ICloudBlob trashImageClient,
            [EventGrid(TopicEndpointUri = "TopicEndpoint", TopicKeySetting = "TopicKey")] IAsyncCollector<EventGridEvent> eventCollector,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}");

            ImageAnalyzerResult result;

            using (var stream = await inputImageClient.OpenReadAsync())
            {
                result = await imageAnalyzer.AnalyzeImageAsync(stream);
            }

            log.LogTrace($"Image Analyzed: result={result?.IsRecognized}");

            if (result != null)
            {
                ICloudBlob clientToUse;
                if (result.IsRecognized)
                {
                    var acceptedLabel = configuration.GetValue<string>("AcceptedLabel");

                    if (result.HasLabel(acceptedLabel))
                    {
                        clientToUse = validImageClient;
                    }
                    else
                    {
                        clientToUse = trashImageClient;
                    }
                }
                else
                {
                    clientToUse = trashImageClient;
                }

                log.LogTrace($"Image Copying: clientToUse={clientToUse.Name}");
                using (var stream = await inputImageClient.OpenReadAsync())
                {
                    await clientToUse.UploadFromStreamAsync(stream);
                }

                var @event = new EventGridEvent(
                   subject: inputImageClient.Uri.ToString() ,
                   eventType: result.IsRecognized ? "imageRecognized" : "imageNotRecognized",
                   dataVersion: "1.0",
                   data: (inputImageClient,result));

                await eventCollector.AddAsync(@event);


                log.LogTrace($"Image Deleting: clientToUse={inputImageClient.Name}");
                await inputImageClient.DeleteAsync();
            }
            else
            {
                throw new Exception();
            }

        }
#endif
    }
}

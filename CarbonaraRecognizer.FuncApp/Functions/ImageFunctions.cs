#define FUNCTIONSTANDARD

using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.Functions
{
    public class ImageFunctions
    {
        private readonly IImageAnalyzer imageAnalyzer;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient destionationStorageServiceClient;
        private readonly EventGridPublisherClient eventClient;
        private readonly ILogger<ImageFunctions> logger;

        public ImageFunctions(IImageAnalyzer imageAnalyzer, IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory, IAzureClientFactory<EventGridPublisherClient> eventClientFactory,
            ILogger<ImageFunctions> logger)
        {
            this.imageAnalyzer = imageAnalyzer;
            this.configuration = configuration;
            this.destionationStorageServiceClient = blobClientFactory.CreateClient(Constants.DestinationBlobClientName);
            this.eventClient = eventClientFactory.CreateClient(Constants.EventGridClientName);
            this.logger = logger;
        }

#if FUNCTIONSTANDARD
        [Function("ImageUploaded")]
        public async Task Run(
            [BlobTrigger("%SourceContainer%/{name}", Connection = "SourceStorageConnectionString")] BlobClient sourceImage,
            string name)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}");

            ImageAnalyzerResult result;


            using (var imageStream = await sourceImage.OpenReadAsync())
            {
                result = await imageAnalyzer.AnalyzeImageAsync(imageStream);
            }

            logger.LogTrace($"Image Analyzed: result={result?.IsRecognized}");

            if (result != null)
            {
                string containerNameToUse = configuration.GetValue<string>("TrashbinContainer");
;
                if (result.IsRecognized)
                {
                    var acceptedLabel = configuration.GetValue<string>("AcceptedLabel");

                    if (result.HasLabel(acceptedLabel))
                    {
                        containerNameToUse = configuration.GetValue<string>("DestinationContainer");
                    }
                    else
                    {
                        containerNameToUse = configuration.GetValue<string>("TrashbinContainer");
                    }
                }

                var blobContainerClient = destionationStorageServiceClient.GetBlobContainerClient(containerNameToUse);

                logger.LogTrace($"Image Copying: containerNameToUse={containerNameToUse}");
                var destinationBlobClient = blobContainerClient.GetBlobClient(name);
                using (var imageStream = await sourceImage.OpenReadAsync())
                {
                    await destinationBlobClient.UploadAsync(imageStream, true);
                }

                var @event = new EventGridEvent(
                   subject: sourceImage.Uri.ToString(),
                   eventType: result.IsRecognized ? "imageRecognized" : "imageNotRecognized",
                   dataVersion: "1.0",
                   data: (containerNameToUse, result));

                await eventClient.SendEventAsync(@event);


                logger.LogTrace($"Image Deleting: imageName={name}");
                await sourceImage.DeleteAsync();
            }
            else
            {
                throw new Exception();
            }

        }
#endif
    }
}

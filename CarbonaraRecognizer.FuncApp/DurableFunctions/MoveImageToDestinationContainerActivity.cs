using Azure.Storage.Blobs;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class MoveImageToDestinationContainerActivity
    {
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient sourceStorageServiceClient;
        private readonly BlobServiceClient destionationStorageServiceClient;
        private readonly ILogger<MoveImageToDestinationContainerActivity> logger;

        public MoveImageToDestinationContainerActivity(IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory,
            ILogger<MoveImageToDestinationContainerActivity> logger)
        {
            this.configuration = configuration;
            this.sourceStorageServiceClient = blobClientFactory.CreateClient(Constants.SourceBlobClientName);
            this.destionationStorageServiceClient = blobClientFactory.CreateClient(Constants.DestinationBlobClientName);
            this.logger = logger;
        }

        [Function(nameof(MoveImageToDestinationContainerActivity))]
        public  async Task Run(
            [ActivityTrigger] ImageAnalyzerOrchestratorDto orchestratorDto)
        {
            var sourceContainerClient = sourceStorageServiceClient.GetBlobContainerClient(configuration.GetValue<string>("SourceContainer"));
            var sourceBlobClient  = sourceContainerClient.GetBlobClient(orchestratorDto.BlobName);

            var destinationContainerClient = destionationStorageServiceClient.GetBlobContainerClient(configuration.GetValue<string>("DestinationContainer"));
            var destinationBlobClient = destinationContainerClient.GetBlobClient(orchestratorDto.BlobName);

            using (var stream = await sourceBlobClient.OpenReadAsync())
            {
                await destinationBlobClient.UploadAsync(stream);
            }

            await sourceBlobClient.DeleteAsync();
        }
    }
   
}

using Azure.Storage.Blobs;
using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.Core.Interfaces;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using CarbonaraRecognizer.FuncApp.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class AnalyzeImageActivity
    {
        private readonly IImageAnalyzer imageAnalyzer;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient sourceStorageServiceClient;
        private readonly ILogger<AnalyzeImageActivity> logger;

        public AnalyzeImageActivity(IImageAnalyzer imageAnalyzer, IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory, ILogger<AnalyzeImageActivity> logger)
        {
            this.imageAnalyzer = imageAnalyzer;
            this.configuration = configuration;
            this.sourceStorageServiceClient = blobClientFactory.CreateClient(Constants.SourceBlobClientName);
            this.logger = logger;
        }

        [Function(nameof(AnalyzeImageActivity))]
        public  async Task<ImageAnalyzerResult> Run(
            [ActivityTrigger] ImageAnalyzerOrchestratorDto orchestratorDto)
        {
            var containerClient = sourceStorageServiceClient.GetBlobContainerClient(configuration.GetValue<string>("SourceContainer"));
            var blobClient = containerClient.GetBlobClient(orchestratorDto.BlobName);

            ImageAnalyzerResult result;
            using (var stream = await blobClient.OpenReadAsync())
            {
                result = await imageAnalyzer.AnalyzeImageAsync(stream);
            }

            return result;
        }
    }
   
}

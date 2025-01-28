//#define DURABLEFUNCTIONS

using Azure.Storage.Blobs;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class ImageUploadClientcs
    {
        private readonly ILogger<ImageUploadClientcs> logger;

        public ImageUploadClientcs(ILogger<ImageUploadClientcs> logger)
        {
            this.logger = logger;
        }

#if DURABLEFUNCTIONS
        [Function("ImageUploadClientcs")]
        public async Task Run(
            [BlobTrigger("%SourceContainer%/{name}", Connection = "SourceStorageConnectionString")] BlobClient inputImageClient,
            string name,
            [DurableClient] DurableTaskClient client)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}");
            var orchestratorDto = new ImageAnalyzerOrchestratorDto()
            {
                BlobName = inputImageClient.Name
            };

            await client.ScheduleNewOrchestrationInstanceAsync(new TaskName("ImageAnalyzerOrchestrator"), orchestratorDto);
        }
#endif

    }
}

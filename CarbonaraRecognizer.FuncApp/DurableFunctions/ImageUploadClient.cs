//#define DURABLEFUNCTIONS

using System;
using System.IO;
using System.Threading.Tasks;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class ImageUploadClientcs
    {

#if DURABLEFUNCTIONS
        [FunctionName("ImageUploadClientcs")]
        public async Task Run(
            [BlobTrigger("%SourceContainer%/{name}", Connection = "SourceStorageConnectionString")] ICloudBlob inputImageClient,
            string name,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}");
            var orchestratorDto = new ImageAnalyzerOrchestratorDto()
            {
                BlobName = inputImageClient.Name
            };

            await client.StartNewAsync("ImageAnalyzerOrchestrator", orchestratorDto);
        }
#endif

    }
}

using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.Core.Interfaces;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class MoveImageToDestinationContainerActivity
    {
        private readonly IConfiguration configuration;

        public MoveImageToDestinationContainerActivity(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName(nameof(MoveImageToDestinationContainerActivity))]
        public  async Task Run(
            [ActivityTrigger] ImageAnalyzerOrchestratorDto orchestratorDto,
            [Blob("%SourceContainer%", FileAccess.ReadWrite, Connection = "SourceStorageConnectionString")] CloudBlobContainer sourceContainerClient,
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "DestinationStorageConnectionString")] CloudBlobContainer destinationContainerClient, 
            ILogger log)
        {
            var blobSourceReference = sourceContainerClient.GetBlockBlobReference(orchestratorDto.BlobName);
            var blobDestinationReference = destinationContainerClient.GetBlockBlobReference(orchestratorDto.BlobName);

            using (var stream = await blobSourceReference.OpenReadAsync())
            {
                await blobDestinationReference.UploadFromStreamAsync(stream);
            }

            await blobSourceReference.DeleteAsync();
        }
    }
   
}

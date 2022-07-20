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
    public class MoveImageToTrashbinContainerActivity
    {
        private readonly IConfiguration configuration;

        public MoveImageToTrashbinContainerActivity(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName(nameof(MoveImageToTrashbinContainerActivity))]
        public  async Task Run(
            [ActivityTrigger] ImageAnalyzerOrchestratorDto orchestratorDto,
            [Blob("%SourceContainer%", FileAccess.ReadWrite, Connection = "SourceStorageConnectionString")] CloudBlobContainer sourceContainerClient,
            [Blob("%TrashbinContainer%", FileAccess.ReadWrite, Connection = "DestinationStorageConnectionString")] CloudBlobContainer trashbinContainerClient, 
            ILogger log)
        {
            var blobSourceReference = sourceContainerClient.GetBlockBlobReference(orchestratorDto.BlobName);
            var blobTrashbinReference = trashbinContainerClient.GetBlockBlobReference(orchestratorDto.BlobName);

            using (var stream = await blobSourceReference.OpenReadAsync())
            {
                await blobTrashbinReference.UploadFromStreamAsync(stream);
            }

            await blobSourceReference.DeleteAsync();
        }
    }
   
}

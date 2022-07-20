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
    public class AnalyzeImageActivity
    {
        private readonly IImageAnalyzer imageAnalyzer;
        private readonly IConfiguration configuration;

        public AnalyzeImageActivity(IImageAnalyzer imageAnalyzer, IConfiguration configuration)
        {
            this.imageAnalyzer = imageAnalyzer;
            this.configuration = configuration;
        }

        [FunctionName(nameof(AnalyzeImageActivity))]
        public  async Task<ImageAnalyzerResult> Run(
            [ActivityTrigger] ImageAnalyzerOrchestratorDto orchestratorDto,
            [Blob("%SourceContainer%", FileAccess.ReadWrite, Connection = "SourceStorageConnectionString")] CloudBlobContainer containerClient,
            ILogger log)
        {
            var blobReference = containerClient.GetBlockBlobReference(orchestratorDto.BlobName);

            ImageAnalyzerResult result;
            using (var stream = await blobReference.OpenReadAsync())
            {
                result = await imageAnalyzer.AnalyzeImageAsync(stream);
            }

            return result;
        }
    }
   
}

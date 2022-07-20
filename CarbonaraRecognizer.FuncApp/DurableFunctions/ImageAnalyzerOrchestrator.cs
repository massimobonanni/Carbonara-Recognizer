using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class ImageAnalyzerOrchestrator
    {
        private readonly IConfiguration configuration;

        public ImageAnalyzerOrchestrator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName(nameof(ImageAnalyzerOrchestrator))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var orchestratorDto = context.GetInput<ImageAnalyzerOrchestratorDto>();

            var analyzeImageResult = await context.CallActivityAsync<ImageAnalyzerResult>(nameof(AnalyzeImageActivity), orchestratorDto);

            var isValid = await context.CallActivityAsync<bool>(nameof(CheckImageActivity), analyzeImageResult);

            if (isValid)
            {
                await context.CallActivityAsync(nameof(MoveImageToDestinationContainerActivity), orchestratorDto);
            }
            else
            {
                await context.CallActivityAsync(nameof(MoveImageToTrashbinContainerActivity), orchestratorDto);
            }
        }

    }
}
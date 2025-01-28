using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class ImageAnalyzerOrchestrator
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ImageAnalyzerOrchestrator> logger;

        public ImageAnalyzerOrchestrator(IConfiguration configuration, ILogger<ImageAnalyzerOrchestrator> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        [Function(nameof(ImageAnalyzerOrchestrator))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
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
using CarbonaraRecognizer.Core.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions
{
    public class CheckImageActivity
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<CheckImageActivity> logger;
            
        public CheckImageActivity(IConfiguration configuration, ILogger<CheckImageActivity> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        [Function(nameof(CheckImageActivity))]
        public bool Run(
            [ActivityTrigger] ImageAnalyzerResult imageAnalyzerResult)
        {
            var retVal = false;
            if (imageAnalyzerResult != null)
            {
                if (imageAnalyzerResult.IsRecognized)
                {
                    var acceptedLabel = configuration.GetValue<string>("AcceptedLabel");

                    if (imageAnalyzerResult.HasLabel(acceptedLabel))
                    {
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
    }

}

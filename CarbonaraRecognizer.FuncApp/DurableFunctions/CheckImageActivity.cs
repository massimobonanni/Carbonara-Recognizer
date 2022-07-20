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
    public class CheckImageActivity
    {
        private readonly IConfiguration configuration;

        public CheckImageActivity(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName(nameof(CheckImageActivity))]
        public bool Run(
            [ActivityTrigger] ImageAnalyzerResult imageAnalyzerResult,
            ILogger log)
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

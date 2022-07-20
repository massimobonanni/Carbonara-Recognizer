using CarbonaraRecognizer.Core.Entities;
using CarbonaraRecognizer.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace CarbonaraRecognizer.CustomVision.Services
{
    public class CustomVisionImageAnalyzer : IImageAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "ImageAnalyzer";
            public string PredictionEndpoint { get; set; }
            public string PredictionKey { get; set; }
            public string ProjectId { get; set; }
            public string ModelName { get; set; }
            public double Threshold { get; set; }

            public static Configuration Load(IConfiguration config)
            {
                var retVal = new Configuration();
                retVal.PredictionEndpoint = config[$"{ConfigRootName}:PredictionEndpoint"];
                retVal.PredictionKey = config[$"{ConfigRootName}:PredictionKey"];
                retVal.ProjectId = config[$"{ConfigRootName}:ProjectId"];
                retVal.ModelName = config[$"{ConfigRootName}:ModelName"];
                retVal.Threshold = config.GetValue<double>($"{ConfigRootName}:Threshold");
                return retVal;
            }
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<CustomVisionImageAnalyzer> logger;

        public CustomVisionImageAnalyzer(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<CustomVisionImageAnalyzer>();
        }

        private CustomVisionPredictionClient CreateVisionClient(Configuration config)
        {
            return new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(config.PredictionKey))
            {
                Endpoint = config.PredictionEndpoint
            };
        }

        private ImageAnalyzerResult ElaborateImagePrediction(ImagePrediction imagePrediction, Configuration config)
        {
            var imageResult = new ImageAnalyzerResult() { IsRecognized = false, Labels = new List<LabelResult>() };

            foreach (var prediction in imagePrediction.Predictions)
            {
                if (prediction.Probability >= config.Threshold)
                {
                    imageResult.IsRecognized = true;
                    imageResult.Labels.Add(new LabelResult()
                    {
                        Confidence = prediction.Probability,
                        Label = prediction.TagName
                    });
                }
            }

            return imageResult;
        }

        public async Task<ImageAnalyzerResult> AnalyzeImageAsync(Stream imageData, CancellationToken token = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var visionClient = CreateVisionClient(config);
                var imagePrediction = await visionClient.ClassifyImageAsync(new Guid(config.ProjectId),
                            config.ModelName, imageData, null, token);
                return ElaborateImagePrediction(imagePrediction, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during custom vision prediction service");
                throw;
                //return ImageAnalyzerResult.Empty;
            }
        }

        public async Task<ImageAnalyzerResult> AnalyzeImageUrlAsync(string imageUrl, CancellationToken token = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var visionClient = CreateVisionClient(config);
                var imagePrediction = await visionClient.ClassifyImageUrlAsync(new Guid(config.ProjectId),
                            config.ModelName, new ImageUrl(imageUrl), null, token);
                return ElaborateImagePrediction(imagePrediction, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during custom vision prediction service");
                throw;
                //return ImageAnalyzerResult.Empty;
            }
        }

    }
}

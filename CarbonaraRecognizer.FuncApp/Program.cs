using Azure;
using CarbonaraRecognizer.Core.Interfaces;
using CarbonaraRecognizer.CustomVision.Services;
using CarbonaraRecognizer.FuncApp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext,services) =>
    {
        services.AddLogging();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<IImageAnalyzer, CustomVisionImageAnalyzer>();

        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(hostContext.Configuration["SourceStorageConnectionString"])
                .WithName(Constants.SourceBlobClientName);
            builder.AddBlobServiceClient(hostContext.Configuration["DestinationStorageConnectionString"])
                .WithName(Constants.DestinationBlobClientName);
            builder.AddEventGridPublisherClient(new Uri(hostContext.Configuration["TopicEndpoint"]),
                    new AzureKeyCredential(hostContext.Configuration["TopicKey"]))
                .WithName(Constants.EventGridClientName);
        });
    })
    .Build();

host.Run();
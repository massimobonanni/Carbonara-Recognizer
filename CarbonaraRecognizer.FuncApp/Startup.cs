using CarbonaraRecognizer.Core.Interfaces;
using CarbonaraRecognizer.CustomVision.Services;
using CarbonaraRecognizer.FuncApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]

namespace CarbonaraRecognizer.FuncApp
{
    public class Startup:FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IImageAnalyzer, CustomVisionImageAnalyzer>();

        }
    }
}

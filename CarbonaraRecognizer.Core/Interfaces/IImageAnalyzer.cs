using CarbonaraRecognizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonaraRecognizer.Core.Interfaces
{
    public interface IImageAnalyzer
    {
        Task<ImageAnalyzerResult> AnalyzeImageAsync(Stream imageData,CancellationToken token = default);

        Task<ImageAnalyzerResult> AnalyzeImageUrlAsync(string imageUrl, CancellationToken token = default);
    }
}

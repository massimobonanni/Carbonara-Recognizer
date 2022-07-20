using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonaraRecognizer.FuncApp.DurableFunctions.Dtos
{
    public class ImageMoveDto
    {
        public string BlobName { get; set; }
        public string DestinationContainer { get; set; }
    }
}

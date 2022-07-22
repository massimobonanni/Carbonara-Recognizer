using CarbonaraRecognizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CarbonaraRecognizer.Core.Test
{
    internal static class TestUtility
    {
        public static LabelResult CreateLabelResult(string label, double confidence = 0.5)
        {
            return new LabelResult() { Label = label, Confidence = confidence };
        }

        public static List<LabelResult> GenerateLabels(params string[] labels)
        {
            return labels.Select(l => CreateLabelResult(l)).ToList();
        }
    }
}

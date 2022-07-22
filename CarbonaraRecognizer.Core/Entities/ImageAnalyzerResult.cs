namespace CarbonaraRecognizer.Core.Entities
{
    public class ImageAnalyzerResult
    {
        public List<LabelResult> Labels { get; set; }
        public bool IsRecognized { get; set; }

        public static ImageAnalyzerResult Empty = new ImageAnalyzerResult() { IsRecognized = false };

        public bool HasLabel(string label, bool ignoreCase = true, int rank = 1)
        { 
            if (this.Labels == null)
                return false;

            var query = this.Labels
                .OrderByDescending(l=>l.Confidence)
                .Take(rank)
                .Where(l => string.Compare(l.Label, label, ignoreCase) == 0);

            return query.Any();
        }
    }

    public class LabelResult
    {
        public string Label { get; set; }
        public double Confidence { get; set; }
    }
}
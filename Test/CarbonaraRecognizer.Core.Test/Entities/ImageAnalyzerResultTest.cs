using CarbonaraRecognizer.Core.Entities;
using System.Collections;

namespace CarbonaraRecognizer.Core.Test.Entities
{
    public class ImageAnalyzerResultTest
    {

        public static IEnumerable<object[]> TestDataForLabelsNull()
        {
            yield return new object[] { null, true, 1, false };
            yield return new object[] { null, false, 1, false };
            yield return new object[] { string.Empty, true, 1, false };
            yield return new object[] { string.Empty, false, 1, false };
            yield return new object[] { "1", true, 1, false };
            yield return new object[] { "1", false, 1, false };
        }
        public static IEnumerable<object[]> TestDataForLabelsNotNull()
        {
            yield return new object[] { TestUtility.GenerateLabels("a","b"), "c",true, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "c", false, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "C", true, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "C", false, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "a", true, 1, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "a", false, 1, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "A", true, 1, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "A", false, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "a", true, 2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "a", false,2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "A", true, 2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "A", false, 2, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "b", true, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "b", false, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "b", true, 2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "b", false, 2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "B", true, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "B", false, 1, false };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "B", true, 2, true };
            yield return new object[] { TestUtility.GenerateLabels("a", "b"), "B", false, 2, false };

        }

        [Theory]
        [MemberData(nameof(TestDataForLabelsNull))]
        public void HasLabel_LabelsNull(string label, bool ignoreCase, int rank, bool expected)
        {
            var target = new ImageAnalyzerResult() { Labels = null };

            var actual = target.HasLabel(label, ignoreCase, rank);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestDataForLabelsNotNull))]
        public void HasLabel_LabelsNotNull(List<LabelResult> labels,string label, bool ignoreCase, int rank, bool expected)
        {
            var target = new ImageAnalyzerResult() { Labels = labels };

            var actual = target.HasLabel(label, ignoreCase, rank);

            Assert.Equal(expected, actual);
        }
    }
}
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Models;

namespace tests
{
    [TestClass]
    public class TrendTests
    {
        [TestMethod]
        public void Zero()
        {
            var scores = new short[] { };
            var trend = Trend.GetTrend(scores);

            trend.Slope.Should().Be(0);
            trend.Offset.Should().Be(0);
        }

        [TestMethod]
        public void Null()
        {
            var trend = Trend.GetTrend(null);

            trend.Slope.Should().Be(0);
            trend.Offset.Should().Be(0);
        }

        [TestMethod]
        public void Positive()
        {
            var scores = new short[] { 25, 75, 50, 75, 75, 50, 75, 75, 100, 75 };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void PositiveMax()
        {
            var scores = new short[] { 0, 100, 100, 100, 100, 100, 100, 100, 100, 100 };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void Negative()
        {
            var scores = new short[] { 90, 80, 80, 80, 90, 50, 70, 60, 50, 55 };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void NegativeMax()
        {
            var scores = new short[] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void Neutral()
        {
            var scores = new short[] { 70, 70, 70, 70, 70, 70, 70, 70, 70, 70 };
            var trend = Trend.GetTrend(scores);
        }
    }
}

using System;

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
            var scores = new ScoreHistoryItem[] { };
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
            var scores = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 25),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 75),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-7), 50),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-6), 75),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-5), 75),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 50),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-3), 75),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-2), 75),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 100),
                new ScoreHistoryItem(DateTime.UtcNow, 75),
            };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void PositiveMax()
        {
            var scores = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-7), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-6), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-5), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-3), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-2), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 100),
                new ScoreHistoryItem(DateTime.UtcNow, 100),
            };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void Negative()
        {
            var scores = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 90),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 80),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-7), 80),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-6), 80),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-5), 90),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 50),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-3), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-2), 60),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 50),
                new ScoreHistoryItem(DateTime.UtcNow, 55),
            };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void NegativeMax()
        {
            var scores = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 100),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-7), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-6), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-5), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-3), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-2), 0),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 0),
                new ScoreHistoryItem(DateTime.UtcNow, 0),
            };
            var trend = Trend.GetTrend(scores);
        }

        [TestMethod]
        public void Neutral()
        {
            var scores = new ScoreHistoryItem[]
            {
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-9), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-8), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-7), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-6), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-5), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-4), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-3), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-2), 70),
                new ScoreHistoryItem(DateTime.UtcNow.AddDays(-1), 70),
                new ScoreHistoryItem(DateTime.UtcNow, 70),
            };
            var trend = Trend.GetTrend(scores);
        }
    }
}

using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Models;

namespace tests
{
    [TestClass]
    public class CountersSummaryTests
    {
        [TestMethod]
        public void CountersSummaryReturnsCorrectTotal()
        {
            // Arrange
            var randomizer = new Random();
            var summary = new CountersSummary
            {
                NoData = randomizer.Next(0, 9),
                Failed = randomizer.Next(10, 99),
                Warning = randomizer.Next(100, 999),
                Passed = randomizer.Next(1000, 9999),
            };

            // Act & Assert
            summary.Total.Should().Be(summary.Warning + summary.Failed + summary.NoData + summary.Passed);
        }

        [TestMethod]
        public void CountersSummaryReturnsCorrectScore()
        {
            // Arrange
            var randomizer = new Random();
            var summary = new CountersSummary
            {
                NoData = randomizer.Next(0, 9),
                Failed = randomizer.Next(10, 99),
                Warning = randomizer.Next(100, 999),
                Passed = randomizer.Next(1000, 9999),
            };

            var expected = Convert.ToInt16(Math.Round(
                100M * summary.Passed * 2 / ((summary.Failed * 2) + (summary.Passed * 2) + summary.Warning),
                MidpointRounding.AwayFromZero));

            // Act & Assert
            summary.Score.Should().Be(expected);
        }

        [TestMethod]
        public void CountersSummaryReturnsZeroIfCountersZero()
        {
            // Arrange
            var randomizer = new Random();
            var summary = new CountersSummary
            {
                NoData = randomizer.Next(0, 1000),
                Failed = 0,
                Warning = 0,
                Passed = 0,
            };

            // Act & Assert
            summary.Score.Should().Be(0);
        }
    }
}
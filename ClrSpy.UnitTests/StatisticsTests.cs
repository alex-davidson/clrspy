using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    public class StatisticsTests
    {
        private static readonly List<ulong> set = new List<ulong> {
            0, 3, 4, 1, 2,
            7, 9, 3, 4, 5,
            5, 5, 6, 3, 1,
            1, 2, 9, 2, 9,
            3, 4
        };

        [Test]
        public void TestSetsAreEquivalent()
        {
            Assert.That(sortedSet, Is.EquivalentTo(set));
        }

        [Test]
        public void StatisticsAreCorrect()
        {
            var stats = Statistics.CalculateInPlace(set.ToArray());

            Assert.That(stats.Total, Is.EqualTo(88));
            Assert.That(stats.Count, Is.EqualTo(22));
            Assert.That(stats.Minimum, Is.EqualTo(0));
            Assert.That(stats.Maximum, Is.EqualTo(9));
            
            Assert.That(stats.Mean, Is.EqualTo(4));
            Assert.That(stats.Median, Is.EqualTo(3.5));

            Assert.That(stats.Variance, Is.EqualTo(150d / 22));
            Assert.That(stats.StandardDeviation, Is.EqualTo(Math.Sqrt(150d / 22)));
        }

        [Test]
        public void CalculatesMedianOfEvenSizedSet()
        {
            var stats = Statistics.CalculateInPlace(new ulong[] { 1, 2, 4, 8 });
            Assert.That(stats.Median, Is.EqualTo(3));
        }
        
        [Test]
        public void CalculatesMedianOfOddSizedSet()
        {
            var stats = Statistics.CalculateInPlace(new ulong[] { 1, 2, 4 });
            Assert.That(stats.Median, Is.EqualTo(2));
        }

        // Sorted version of the test set, for easier manual verification of results:
        private readonly List<ulong> sortedSet = new List<ulong> {
            0,
            1, 1, 1,
            2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4,
            5, 5, 5,
            6,
            7,
            9, 9, 9
        };
        /*
            Sum : 88
            Count: 22
            Mean: 4
            Sum of squared differences from the mean: 150
        */

    }
}

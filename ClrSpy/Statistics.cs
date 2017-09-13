using System;
using System.Linq;

namespace ClrSpy
{
    public class Statistics
    {   
        // NOTE: Modifies the input array.
        public static Statistics CalculateInPlace(ulong[] values)
        {
            if(values.LongLength == 0) throw new ArgumentException("Empty array", nameof(values));
            Array.Sort(values);

            var stats = new Statistics {
                Count = (ulong)values.LongLength,
                Minimum = values.First(),
                Maximum = values.Last(),
                Median = GetMedian(values)
            };

            for(long i = 0; i < values.LongLength; i++)
            {
                stats.Total += values[i];
            }
            stats.Mean = (double)stats.Total / stats.Count;
            stats.Variance = GetVariance(values, stats.Mean);
            stats.StandardDeviation = Math.Sqrt(stats.Variance);

            return stats;
        }

        private static double GetVariance(ulong[] values, double mean)
        {
            double sum = 0;
            double sumOfSquares = 0;
            for(long i = 0; i < values.LongLength; i++)
            {
                var d = values[i] - mean;
                sum += d;
                sumOfSquares += d*d;
            }
            var squareOfSums = sum * sum; 
            // Note: this is the population variance, not the sample variance.
            return (sumOfSquares - (squareOfSums / values.LongLength)) / values.LongLength;
        }

        private static double GetMedian(ulong[] array)
        {
            if(array.LongLength % 2 == 1)
            {
                var pos = (array.LongLength - 1) / 2;
                return array[pos];
            }
            else
            {
                var pos = array.LongLength / 2;
                return (double)(array[pos] + array[pos - 1]) / 2;
            }
        }

        public ulong Count { get; set; }
        public ulong Total { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        /// <summary>
        /// Population variance.
        /// </summary>
        public double Variance { get; set; }
        public double StandardDeviation { get; set; }
        public ulong Maximum { get; set; }
        public ulong Minimum { get; set; }
    }
}

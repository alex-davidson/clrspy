using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClrSpy.HeapAnalysis.Collections;
using ClrSpy.UnitTests.Utils;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Collections
{
    [TestFixture]
    public class ObjectAddressSetTests
    {
        [Test]
        public void AddingAddressInsideHeap_ForTheFirstTime_ReturnsTrue()
        {
            var set = new ObjectAddressSet(GetTestHeapIndex());

            Assert.That(set.Add(0x00200000), Is.True);
        }

        [Test]
        public void AddingAddressInsideHeap_ForTheSecondTime_ReturnsFalse()
        {
            var set = new ObjectAddressSet(GetTestHeapIndex());
            
            Assert.That(set.Add(0x00200000), Is.True);
            Assert.That(set.Add(0x00200000), Is.False);
        }

        [Test]
        public void AddingAddressOutsideHeap_ForTheFirstTime_ReturnsTrue()
        {
            var set = new ObjectAddressSet(GetTestHeapIndex());

            Assert.That(set.Add(0x00400000), Is.True);
        }

        [Test]
        public void AddingAddressOutsideHeap_ForTheSecondTime_ReturnsFalse()
        {
            var set = new ObjectAddressSet(GetTestHeapIndex());

            Assert.That(set.Add(0x00400000), Is.True);
            Assert.That(set.Add(0x00400000), Is.False);
        }

        [Test, Explicit]
        [TestCaseSource(typeof(RandomSeed), nameof(RandomSeed.Value))] 
        public void CollectionIsFastEnoughForAMillionAddresses(int seed)
        {
            var random = new Random(seed);
            var addresses = Enumerable.Range(0, 1000000).Select(i => (ulong)random.Next(0x00100000, 0x28000000)).ToArray();

            var collection = new ObjectAddressSet(GetTestHeapIndex());
            var swObjectAddressSet = Stopwatch.StartNew();
            foreach (var address in addresses) collection.Add(address);
            foreach (var address in addresses) collection.Contains(address);
            swObjectAddressSet.Stop();

            var hashSet = new HashSet<ulong>();
            var swHashSet = Stopwatch.StartNew();
            foreach (var address in addresses) hashSet.Add(address);
            foreach (var address in addresses) hashSet.Contains(address);
            swHashSet.Stop();

            Trace.WriteLine($"ObjectAddressSet: {swObjectAddressSet.Elapsed}");
            Trace.WriteLine($"HashSet<ulong>:   {swHashSet.Elapsed}");
        }

        private static HeapIndex GetTestHeapIndex() => new HeapIndex(GetTestHeapSegments(), 3);
        private static HeapIndex.SegmentBounds[] GetTestHeapSegments()
        {
            return new [] {
                HeapIndex.InvalidSegment,
                new HeapIndex.SegmentBounds { Low = 0x00100000, High = 0x00300000 },
                new HeapIndex.SegmentBounds { Low = 0x00440000, High = 0x004C0000 },
                new HeapIndex.SegmentBounds { Low = 0x00500000, High = 0x01000000 },
                new HeapIndex.SegmentBounds { Low = 0x10000000, High = 0x28000000 }
            };
        }
    }
}

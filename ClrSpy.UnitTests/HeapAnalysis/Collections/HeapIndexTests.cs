using ClrSpy.HeapAnalysis.Collections;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Collections
{
    [TestFixture]
    public class HeapIndexTests
    {
        [TestCase(0x6000UL)]
        [TestCase(0x47FFUL)]
        [TestCase(0x47FCUL)]
        public void AddressOutsideOfHeapSegments_Yields_InvalidKey(ulong address)
        {
            var index = Create32BitHeapIndex(
                HeapIndex.InvalidSegment,
                new HeapIndex.SegmentBounds { Low = 0x4800, High = 0x6000 },
                new HeapIndex.SegmentBounds { Low = 0xC000, High = 0x10000 });

            Assert.That(index.ResolveToKey(address), Is.EqualTo(HeapIndex.Key.Invalid));
        }

        [TestCase(0x2400UL, 2, 128)]
        [TestCase(0x1FFCUL, 1, 511)]
        // Boundary cases, where '>' vs '>=' will cause the wrong segment to be chosen.
        [TestCase(0x2000UL, 2, 0)]
        [TestCase(0x8000UL, 4, 0)]
        // Lowermost valid key.
        [TestCase(0x1000UL, 1, 0)]
        // Uppermost valid key.
        [TestCase(0xBFFFUL, 4, 2047)]
        public void ResolvesAddressToCorrect32BitSegmentAndSlot(ulong address, int segment, int slot)
        {
            var index = Create32BitHeapIndex(
                HeapIndex.InvalidSegment,
                new HeapIndex.SegmentBounds { Low = 0x1000, High = 0x2000 },
                new HeapIndex.SegmentBounds { Low = 0x2000, High = 0x3000 },
                new HeapIndex.SegmentBounds { Low = 0x7000, High = 0x8000 },
                new HeapIndex.SegmentBounds { Low = 0x8000, High = 0xC000 });

            Assert.That(index.ResolveToKey(address), Is.EqualTo(new HeapIndex.Key(segment, slot)));
        }

        [TestCase(0x2400UL, 2, 64)]
        [TestCase(0x1FFCUL, 1, 255)]
        // Boundary cases, where '>' vs '>=' will cause the wrong segment to be chosen.
        [TestCase(0x2000UL, 2, 0)]
        [TestCase(0x8000UL, 4, 0)]
        public void ResolvesAddressToCorrect64BitSegmentAndSlot(ulong address, int segment, int slot)
        {
            var index = Create64BitHeapIndex(
                HeapIndex.InvalidSegment,
                new HeapIndex.SegmentBounds { Low = 0x1000, High = 0x2000 },
                new HeapIndex.SegmentBounds { Low = 0x2000, High = 0x3000 },
                new HeapIndex.SegmentBounds { Low = 0x7000, High = 0x8000 },
                new HeapIndex.SegmentBounds { Low = 0x8000, High = 0xC000 });

            Assert.That(index.ResolveToKey(address), Is.EqualTo(new HeapIndex.Key(segment, slot)));
        }

        [TestCase(0x1F27FE10UL, 7, 2096578)]
        public void Specific32BitTestCases(ulong address, int segment, int slot)
        {
            var visualStudioHeapIndex = Create32BitHeapIndex(
                HeapIndex.InvalidSegment,
                new HeapIndex.SegmentBounds { Low = 0x034B1000, High = 0x044AFFF4 },
                new HeapIndex.SegmentBounds { Low = 0x044B1000, High = 0x05477E88 },
                new HeapIndex.SegmentBounds { Low = 0x17E31000, High = 0x18E2FF80 },
                new HeapIndex.SegmentBounds { Low = 0x19651000, High = 0x1A64FF64 },
                new HeapIndex.SegmentBounds { Low = 0x1B281000, High = 0x1C27FF0C },
                new HeapIndex.SegmentBounds { Low = 0x1C281000, High = 0x1D27FEBC },
                new HeapIndex.SegmentBounds { Low = 0x1E281000, High = 0x1F27FE1C }, // 7
                new HeapIndex.SegmentBounds { Low = 0x1FDA1000, High = 0x20D9FFB0 },
                new HeapIndex.SegmentBounds { Low = 0x20DA1000, High = 0x21D9FFA0 },
                new HeapIndex.SegmentBounds { Low = 0x21DA1000, High = 0x22D9FE20 },
                new HeapIndex.SegmentBounds { Low = 0x284A1000, High = 0x2949FF9C },
                new HeapIndex.SegmentBounds { Low = 0x2DC21000, High = 0x2EC1FFE4 },
                new HeapIndex.SegmentBounds { Low = 0x31F31000, High = 0x32F25D30 },
                new HeapIndex.SegmentBounds { Low = 0x351B1000, High = 0x361AFCE8 },
                new HeapIndex.SegmentBounds { Low = 0x38E81000, High = 0x39E72930 },
                new HeapIndex.SegmentBounds { Low = 0x3D7E1000, High = 0x3E7DD228 },
                new HeapIndex.SegmentBounds { Low = 0x3F421000, High = 0x4041FF90 },
                new HeapIndex.SegmentBounds { Low = 0x40831000, High = 0x4182F2B8 },
                new HeapIndex.SegmentBounds { Low = 0x41F31000, High = 0x4273B024 },
                new HeapIndex.SegmentBounds { Low = 0x45291000, High = 0x46039EA8 },
                new HeapIndex.SegmentBounds { Low = 0x4E401000, High = 0x4E989088 },
                new HeapIndex.SegmentBounds { Low = 0x51BA1000, High = 0x52B92258 });

            Assert.That(visualStudioHeapIndex.ResolveToKey(address), Is.EqualTo(new HeapIndex.Key(segment, slot)));
        }

        private static HeapIndex Create32BitHeapIndex(params HeapIndex.SegmentBounds[] segments) => new HeapIndex(segments, 3);
        private static HeapIndex Create64BitHeapIndex(params HeapIndex.SegmentBounds[] segments) => new HeapIndex(segments, 4);
    }
}

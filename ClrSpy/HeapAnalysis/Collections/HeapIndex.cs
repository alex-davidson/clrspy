using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Collections
{
    /// <summary>
    /// Defines a mapping from a 64-bit absolute address to a more compact representation, based
    /// on the structure of the CLR heap.
    /// </summary>
    /// <remarks>
    /// Based on ObjectSet, from the DumpHeapLive sample project of CLRMD.
    /// </remarks>
    public struct HeapIndex
    {
        private readonly SegmentBounds[] segments;
        private readonly int shift;

        public static HeapIndex Create(ClrHeap heap)
        {
            var segments = new SegmentBounds[heap.Segments.Count + 1];
#if DEBUG
            ulong last = 0;
#endif
            for (var i = 0; i < heap.Segments.Count; i++)
            {
                var seg = heap.Segments[i];
#if DEBUG
                // Check that the segments are in order.
                Debug.Assert(last < seg.Start);
                // If the segment is too large, we can't compact the offset into a 32-bit signed int.
                Debug.Assert(seg.End - seg.Start < int.MaxValue);
                last = seg.Start;
#endif
                // We contrive to ensure that the default value of HeapIndex.Key
                // is 'invalid' by ensuring that segment 0 is always invalid.
                segments[i + 1].Low = seg.Start;
                segments[i + 1].High = seg.End;
            }
            return new HeapIndex(segments, IntPtr.Size == 4 ? 3 : 4);
        }

        /// <summary>
        /// Constructor, used internally and for testing. Use `HeapIndex.Create(heap)` instead.
        /// </summary>
        public HeapIndex(SegmentBounds[] segments, int shift)
        {
            if (!Equals(segments[0], InvalidSegment)) throw new ArgumentException("First SegmentBounds is required to be invalid.", nameof(segments));
            this.segments = segments;
            this.shift = shift;
        }

        [Pure]
        public Key ResolveToKey(ulong address)
        {
            if (address == 0) return Key.Invalid;

            var segment = GetSegmentNumber(address);
            if (segment < 0) return Key.Invalid;

            var slot = GetSlot(address - segments[segment].Low);
            AssertThatKeyIsSane(address, segment, slot);
            return new Key(segment, slot);
        }

        private void AssertThatKeyIsSane(ulong address, int segment, int slot)
        {
            if (slot >= GetSlotCount(segment))
            {
                Debug.Fail($"Unable to sanely resolve address {address:X16}: got Seg[{segment}][{slot}].\n{DumpSegmentBounds()}");
            }
        }

        private string DumpSegmentBounds()
        {
            return String.Join("\n", segments.Select((s, i) => $"Seg[{i}]: {s.Low:X16} - {s.High:X16}"));
        }

        [Pure]
        public T[] CreateSegmentTable<T>(Func<int, T> create)
        {
            var table = new T[segments.Length];
            for (var i = 1; i < table.Length; i++) table[i] = create(GetSlotCount(i));
            return table;
        }

        [Pure]
        public T[] CreateSegmentTable<T>() where T : new()
        {
            var table = new T[segments.Length];
            for (var i = 1; i < table.Length; i++) table[i] = new T();
            return table;
        }

        [Pure]
        private int GetSlotCount(int segment) => GetSlot(segments[segment].High - segments[segment].Low);

        [Pure]
        private int GetSegmentNumber(ulong value)
        {
            var low = 0;
            var high = segments.Length - 1;

            // Binary search for the appropriate segment:
            while (low <= high)
            {
                var mid = (low + high) >> 1;
                if (value < segments[mid].Low)
                {
                    high = mid - 1;
                }
                else if (value >= segments[mid].High)
                {
                    low = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            // Outside of the heap.
            return -1;
        }

        private int GetSlot(ulong offset)
        {
            Debug.Assert(offset < uint.MaxValue);
            return (int)(offset >> shift);
        }

        public struct Key
        {
            public Key(int segment, int slot)
            {
                Segment = segment;
                Slot = slot;
                if (!IsValid) throw new InvalidOperationException("Cannot construct invalid HeapIndex.Key");
            }

            public int Segment { get; }
            public int Slot { get; }


            public override int GetHashCode() => Slot;
            public bool IsValid => (Segment | Slot) != 0;

            public override string ToString()
            {
                if (IsValid) return $"Seg[{Segment}][{Slot}]";
                return "(invalid)";
            }

            public static readonly Key Invalid = default(Key);
        }

        public static readonly SegmentBounds InvalidSegment = default(SegmentBounds);

        public struct SegmentBounds
        {
            public ulong High;
            public ulong Low;
        }
    }
}

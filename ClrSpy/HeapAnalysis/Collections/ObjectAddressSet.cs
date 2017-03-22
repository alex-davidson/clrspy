using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Collections
{
    /// <summary>
    /// A more memory-efficient Set implementation for object addresses.
    /// </summary>
    /// <remarks>
    /// Based on ObjectSet, from the DumpHeapLive sample project of CLRMD.
    /// </remarks>
    public class ObjectAddressSet
    {
        private readonly BitArray[] segmentBitsets;
        private readonly HashSet<ulong> nonHeapSegment = new HashSet<ulong>();
        private readonly HeapIndex index;

        public ObjectAddressSet(ClrHeap heap) : this(HeapIndex.Create(heap))
        {
        }

        /// <summary>
        /// Used internally and for testing. Use `new ObjectAddressSet(heap)` instead.
        /// </summary>
        public ObjectAddressSet(HeapIndex index)
        {
            this.index = index;
            segmentBitsets = index.CreateSegmentTable(s => new BitArray(s));
        }

        public bool Add(ulong value)
        {
            if (value == 0) throw new ArgumentException();

            var key = index.ResolveToKey(value);
            if (!key.IsValid) return nonHeapSegment.Add(value);

            var segmentBitset = segmentBitsets[key.Segment];
            if (segmentBitset.Get(key.Slot)) return false;
            segmentBitset.Set(key.Slot, true);
            Count++;
            return true;
        }

        public bool Contains(ulong value)
        {
            var key = index.ResolveToKey(value);
            if (!key.IsValid) return nonHeapSegment.Contains(value);
            var segmentBitset = segmentBitsets[key.Segment];
            return segmentBitset[key.Slot];
        }

        public int Count { get; private set; }
    }
}

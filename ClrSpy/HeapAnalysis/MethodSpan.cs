using System;
using System.Linq;

namespace ClrSpy.HeapAnalysis
{
    public struct MethodSpan
    {
        public LineRange[] LineRanges { get; }
        public int BeginLine { get; }
        public int EndLine { get; }

        public MethodSpan(LineRange[] ranges) : this()
        {
            var beginLine = int.MaxValue;
            var endLine = int.MinValue;
            foreach (var range in ranges)
            {
                if (!range.Valid) continue;
                beginLine = Math.Min(range.Begin, beginLine);
                endLine = Math.Max(range.End, endLine);
            }
            if (beginLine <= endLine)
            {
                LineRanges = ranges;
                BeginLine = beginLine;
                EndLine = endLine;
            }
        }

        public bool Overlaps(MethodSpan other)
        {
            if (LineRanges == null) return false;
            if (other.LineRanges == null) return false;
            if (other.BeginLine > this.EndLine) return false;
            if (other.EndLine < this.BeginLine) return false;

            return LineRanges.Any(r => other.LineRanges.Any(o => o.Overlaps(r)));
        }
    }
}

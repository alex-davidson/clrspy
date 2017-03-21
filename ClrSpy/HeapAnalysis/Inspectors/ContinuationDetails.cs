using System;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Inspectors
{
    public class ContinuationDetails
    {
        public string Summary { get; set; }
        public SourceLocation Source { get; set; }
        public ClrMethod SourceMethod { get; set; }
        public ClrMethod ActualMethod { get; set; }
    }

    public struct SourceLocation
    {
        public string File { get; set; }
        public LineRange[] LineRanges { get; set; }

        public override string ToString()
        {
            if (File == null) return "<unknown>";
            if (LineRanges?.Any() == true)
            {
                var ranges = String.Join(",", LineRanges);
                return $"{File}:{ranges}";
            }
            return $"{File}:<unknown>";
        }
    }

    public struct LineRange
    {
        public int Begin { get; set; }
        public int End { get; set; }

        public override string ToString()
        {
            return Begin == End ? Begin.ToString() : $"{Begin}-{End}";
        }
    }
}

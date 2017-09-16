namespace ClrSpy.HeapAnalysis
{
    public struct LineRange
    {
        public int Begin { get; }
        public int End { get; }
        public bool Valid { get; }

        public LineRange(int begin, int end) : this()
        {
            if (begin > 0 && begin != LineInvalid &&
                end > 0 && end != LineInvalid)
            {
                Begin = begin;
                End = end;
                Valid = true;
            }
        }

        public bool Overlaps(LineRange other)
        {
            if (!Valid || !other.Valid) return false;
            if (other.Begin > this.End) return false;
            if (other.End < this.Begin) return false;
            return true;
        }

        public override string ToString()
        {
            return Begin == End? Begin.ToString() : $"{Begin}-{End}";
        }

        private static int LineInvalid = 0x00feefee;
        public static readonly LineRange InvalidRange = default(LineRange); 
    }
}

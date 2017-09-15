using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public class ClrMethodEqualityComparer : IEqualityComparer<ClrMethod>
    {
        public bool Equals(ClrMethod x, ClrMethod y)
        {
            if (ReferenceEquals(null, x)) return ReferenceEquals(null, y);
            if (ReferenceEquals(null, y)) return false;
            return Equals(x.MetadataToken, y.MetadataToken);
        }

        public int GetHashCode(ClrMethod obj)
        {
            return obj.MetadataToken.GetHashCode();
        }
    }
}

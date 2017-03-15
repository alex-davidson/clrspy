using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public class ClrTypeEqualityComparer : IEqualityComparer<ClrType>
    {
        public static readonly ClrTypeEqualityComparer Instance = new ClrTypeEqualityComparer();

        public bool Equals(ClrType x, ClrType y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

            if (!Equals(x.Name, y.Name)) return false;
            if (!ReferenceEquals(x.Heap, y.Heap)) return false;

            return Equals(x.Module?.AssemblyId, y.Module?.AssemblyId);
        }

        public int GetHashCode(ClrType obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}

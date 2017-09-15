using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public sealed class ClrArrayObject : ClrReferenceObject, IClrObject, IEquatable<ClrArrayObject>, IAddressable
    {
        internal ClrArrayObject(ClrType type, ulong address) : base(type, address)
        {
        }

        public int Length => Type.GetArrayLength(Address);

        public IClrObject GetElement(int index)
        {
            if (Type.ComponentType.IsValueClass) return new ClrStructObject(this, Type.ComponentType, Type.GetArrayElementAddress(Address, index));
            return new ClrObjectReader().ReadFromArrayIndex(Type, Address, index);
        }

        public IEnumerable<TItem> AsEnumerable<TItem>()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return GetElement(i).CastAs<TItem>();
            }
        }

        public override string ToString() => Type.Name;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ClrArrayObject);
        }
        public bool Equals(IClrObject other) => Equals(other as ClrArrayObject);
        public bool Equals(ClrArrayObject other) => Address == other?.Address && ClrObjectUtils.AssertTypesEquivalent(this, other);
        public override int GetHashCode() => Address.GetHashCode();
    }
}

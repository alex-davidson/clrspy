using System;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public sealed class ClrStructObject : IClrCompositeObject, IEquatable<ClrStructObject>
    {
        private readonly ClrReferenceObject container;

        internal ClrStructObject(ClrReferenceObject container, ClrType type, ulong address)
        {
            this.container = container;
            Type = type;
            Address = address;
        }

        public ClrHeap OwningHeap => Type.Heap;
        public ClrType Type { get; }
        public ulong Address { get; }

        public IClrObject GetFieldValue(string fieldName)
        {
            var field = Type.GetFieldByName(fieldName);
            if (field.Type.IsValueClass) return new ClrStructObject(container, field.Type, field.GetAddress(Address, true));
            return new ClrObjectReader().ReadFromFieldValue(field, Address, container != null);
        }

        public override string ToString() => Type.Name;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ClrStructObject);
        }
        public bool Equals(IClrObject other) => Equals(other as ClrStructObject);
        public bool Equals(ClrStructObject other) => Address == other?.Address && ClrObjectUtils.AssertTypesEquivalent(this, other);
        public override int GetHashCode() => Address.GetHashCode();
    }
}

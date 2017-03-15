using System;
using System.Diagnostics;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public sealed class ClrClassObject : ClrReferenceObject, IClrCompositeObject, IEquatable<ClrClassObject>
    {
        internal ClrClassObject(ClrType type, ulong address) : base(type, address)
        {
        }

        public IClrObject GetFieldValue(string fieldName)
        {
            var field = Type.GetFieldByName(fieldName);
            if (field.Type.IsValueClass) return new ClrStructObject(this, field.Type, field.GetAddress(Address, false));
            return new ClrObjectReader().ReadFromFieldValue(field, Address);
        }

        public override string ToString() => Type.Name;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ClrClassObject);
        }
        public bool Equals(IClrObject other) => Equals(other as ClrClassObject);
        public bool Equals(ClrClassObject other) => Address == other?.Address && ClrObjectUtils.AssertTypesEquivalent(this, other);
        public override int GetHashCode() => Address.GetHashCode();
    }
}

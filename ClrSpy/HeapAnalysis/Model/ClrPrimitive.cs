using System;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public sealed class ClrPrimitive : IClrObject, IEquatable<ClrPrimitive>
    {
        internal ClrPrimitive(ClrType type, object value)
        {
            if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
            Type = type;
            Value = value;
        }

        public ClrType Type { get; }
        public object Value { get; }
        public T ValueAs<T>() => (T)Value;

        public override string ToString() => $"{Type.Name}:{Value}";

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ClrStructObject);
        }
        public bool Equals(IClrObject other) => Equals(other as ClrPrimitive);
        public bool Equals(ClrPrimitive other) => Equals(Value, other?.Value) && ClrObjectUtils.AreTypesEquivalent(this, other);
        public override int GetHashCode() => Value.GetHashCode();
    }
}

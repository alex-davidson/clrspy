using System;
using ClrSpy.Native;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public sealed class ClrPrimitive : IClrObject, IEquatable<ClrPrimitive>
    {
        internal ClrPrimitive(ClrType type, object value)
        {
            if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
            Type = type;
            if (type.CanBeAssignedTo<IntPtr>())
            {
                Value = PointerUtils.CastLongToIntPtr((long)value);
            }
            else
            {
                Value = value;
            }
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

    public class ValueReader
    {
        public bool TryReadValue<T>(IClrObject obj, out T value)
        {
            var structObj = obj as ClrStructObject;
            if (structObj?.Type.CanBeAssignedTo<Guid>() == true)
            {
                var bytes = new byte[16];
                var count = structObj.Type.Heap.ReadMemory(structObj.Address, bytes, 0, 16);
                if (count != 16) throw new Exception();
                var guid = new Guid(bytes);
                value = Util.ForceCast<Guid, T>(guid);
                return true;
            }
            value = default(T);
            return false;
        }
    }
}

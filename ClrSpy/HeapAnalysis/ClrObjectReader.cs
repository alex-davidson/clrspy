using System;
using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public class ClrObjectReader
    {
        public IClrObject ReadFromAddress(ClrHeap heap, ulong address)
        {
            var type = heap.GetObjectType(address);
            return ReadFromAddress(type, address);
        }

        public IClrObject ReadFromAddress(ClrType type, ulong address)
        {
            if (address == 0) return null;
            return ReadFromNotNullAddress(type, address);
        }

        public IClrObject ReadGCRoot(ClrRoot root)
        {
            if (root.Address == 0) return null;
            return ReadFromNotNullAddress(root.Type, root.Object);
        }

        public IClrObject ReadFromFieldValue(ClrInstanceField field, ulong address, bool ownerIsNested = false)
        {
            if (field.Type.IsValueClass) throw new ArgumentException("Cannot read ClrValue from field value. Handle this case in the caller.");
            if (field.Type.IsPrimitive || field.Type.IsString)
            {
                var value = field.GetValue(address, ownerIsNested);
                return WrapPrimitiveValue(field.Type, value);
            }

            var instanceAddress = (ulong)field.GetValue(address, ownerIsNested);
            if (instanceAddress == 0) return null;
            var actualType = field.Type.Heap.GetObjectType(instanceAddress);
            return ReadFromNotNullAddress(actualType, instanceAddress);
        }

        public IClrObject ReadFromArrayIndex(ClrType arrayType, ulong baseAddress, int index)
        {
            if (arrayType.ComponentType.IsPrimitive || arrayType.ComponentType.IsString)
            {
                var value = arrayType.GetArrayElementValue(baseAddress, index);
                return WrapPrimitiveValue(arrayType.ComponentType, value);
            }
            var address = (ulong)arrayType.GetArrayElementValue(baseAddress, index);
            if (address == 0) return null;
            var actualType = arrayType.Heap.GetObjectType(address);
            return ReadFromAddress(actualType, address);
        }

        private IClrObject ReadFromNotNullAddress(ClrType type, ulong address)
        {
            if (type.IsArray) return new ClrArrayObject(type, address);
            if (type.IsValueClass) return new ClrStructObject(null, type, address);
            if (type.IsPrimitive || type.IsString)
            {
                var value = type.GetValue(address);
                return WrapPrimitiveValue(type, value);
            }
            return new ClrClassObject(type, address);
        }

        private static IClrObject WrapPrimitiveValue(ClrType type, object value)
        {
            if (Equals(value, null)) return null;
            return new ClrPrimitive(type, value);
        }

        /// <summary>
        /// Returns true if an IClrObject can be constructed around this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsValidObjectType(ClrType type)
        {
            if (type == null) return false; // Heap corruption?
            if (type.IsFree) return false;
            return true;
        }
    }
}

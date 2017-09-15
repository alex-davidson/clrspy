using System;
using System.Linq;
using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public static class ClrObjectExtensions
    {
        public static T GetFieldValue<T>(this IClrCompositeObject instance, string fieldName)
        {
            var value = instance.GetFieldValue(fieldName);
            if (Equals(default(T), value)) return default(T);
            if (TryCastAs(value, out T result)) return result;
            throw new InvalidCastException($"Field '{fieldName}' of {instance} yields a {value.GetType().Name} and cannot be accessed as a {typeof(T).Name}");
        }

        public static T GetElement<T>(this ClrArrayObject arrayObject, int index)
        {
            var value = arrayObject.GetElement(index);
            if (TryCastAs(value, out T result)) return result;
            throw new InvalidCastException($"Element {index} of this {arrayObject} cannot be accessed as a {typeof(T).Name}");
        }

        public static T CastAs<T>(this IClrObject obj)
        {
            if (Equals(default(T), obj)) return default(T);
            if (TryCastAs(obj, out T result)) return result;
            throw new InvalidCastException($"This object is a {obj.GetType().Name} and cannot be accessed as a {typeof(T).Name}");
        }

        private static bool TryCastAs<T>(IClrObject obj, out T value)
        {
            if (obj is T casted)
            {
                value = casted;
                return true;
            }
            if (new ValueReader().TryReadValue(obj, out value)) return true;

            if (obj is ClrPrimitive primitive)
            {
                if (primitive.Value is T)
                {
                    value = primitive.ValueAs<T>();
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public static string DumpFields(this IClrCompositeObject instance)
        {
            return String.Join(
                Environment.NewLine,
                instance.Type.Fields.Select(f => $"{f.Name} -> {instance.GetFieldValue(f.Name)}"));
        }

        public static ClrMethod FindMethodByName(this ClrType clrType, string methodName)
        {
            return clrType.Methods.SingleOrDefault(m => m.Name == methodName);
        }
    }
}

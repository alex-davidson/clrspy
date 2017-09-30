using System;
using System.Diagnostics;
using System.Text;
using ClrSpy.Metadata;

namespace ClrSpy.UnitTests.Metadata
{
    public class MethodSigFormatter
    {
        private static readonly string genericArgNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string Format(string name, MethodSig sig)
        {
            var writer = new StringBuilder();
            writer.Append(name);
            FormatGenericArguments(sig, writer);
            writer.Append("(");
            FormatParameters(sig, writer);
            writer.Append(")");
            return writer.ToString();
        }

        private void FormatParameters(MethodSig sig, StringBuilder writer)
        {
            if (sig.Parameters.Length == 0) return;
            FormatType(sig.Parameters[0].Type, writer);
            for (var i = 1; i < sig.GenericParameterCount; i++) FormatType(sig.Parameters[i].Type, writer);
        }

        private void FormatType(TypeSig type, StringBuilder writer)
        {
            if (type is SimpleTypeSig simpleType)
            {
                writer.Append(simpleType.SimpleType.FullName);
                return;
            }
            if (type is SzArrayTypeSig szArrayType)
            {
                FormatType(szArrayType.ElementType, writer);
                writer.Append("[]");
                return;
            }
            if (type is ComplexTypeSig complexType)
            {
                writer.Append(complexType.IsValueType ? "*STRUCT*" : "*CLASS*");
                return;
            }
            if (type is GenericTypeSig genericType)
            {
                writer.Append(genericType.TypeDefinition.IsValueType ? "*GENERIC STRUCT*" : "*GENERIC CLASS*");
                return;
            }
            if (type is GenericArgumentTypeSig genericArgumentType)
            {
                if (genericArgumentType.IsFromMethod)
                {
                    NameGenericMethodArg(genericArgumentType.Index, writer);
                }
                else
                {
                    NameGenericTypeArg(genericArgumentType.Index, writer);
                }
                return;
            }
            if (type is ArrayTypeSig arrayType)
            {
                FormatType(arrayType.ElementType, writer);
                FormatArrayShape(arrayType.Dimensions, writer);
                return;
            }
            throw new NotImplementedException(type.GetType().FullName);
        }

        private void FormatArrayShape(ArrayDimension[] dimensions, StringBuilder writer)
        {
            writer.Append("[");
            FormatArrayDimension(dimensions[0], writer);
            for (var i = 1; i < dimensions.Length; i++) FormatArrayDimension(dimensions[i], writer);
            writer.Append("]");
        }

        private void FormatArrayDimension(ArrayDimension dimension, StringBuilder writer)
        {
            if (dimension.Size == null) return;
            if (dimension.LowerBound != 0)
            {
                writer.Append(dimension.LowerBound);
                writer.Append("..");
            }
            writer.Append(dimension.UpperBound);
        }

        private void FormatGenericArguments(MethodSig sig, StringBuilder writer)
        {
            if (sig.GenericParameterCount == 0) return;
            writer.Append("<A");
            for (var i = 1; i < sig.GenericParameterCount; i++) NameGenericMethodArg(i, writer);
            writer.Append(">");
        }

        private void NameGenericMethodArg(int index, StringBuilder writer)
        {
            Debug.Assert(index < genericArgNames.Length);
            writer.Append("M");
            writer.Append(genericArgNames[index]);
        }

        private void NameGenericTypeArg(int index, StringBuilder writer)
        {
            Debug.Assert(index < genericArgNames.Length);
            writer.Append("T");
            writer.Append(genericArgNames[index]);
        }
    }
}
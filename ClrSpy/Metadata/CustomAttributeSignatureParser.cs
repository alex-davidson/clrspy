using System;
using System.Diagnostics;

namespace ClrSpy.Metadata
{
    public class CustomAttributeSignatureParser
    {
        private readonly ClrEnumUnderlyingTypeResolver enumTypeResolver;

        public CustomAttributeSignatureParser(ClrEnumUnderlyingTypeResolver enumTypeResolver)
        {
            this.enumTypeResolver = enumTypeResolver;
        }

        public CustomAttributeSig Parse(SignatureReader reader, MethodSig constructorSig)
        {
            if (!reader.TryReadProlog()) throw new SignatureFormatException("Custom attribute prolog was not found.", reader.Offset);
            var sig = new CustomAttributeSig();

            var fixedArgCount = constructorSig.Parameters.Length;
            sig.FixedArgs = new FixedArg[fixedArgCount];
            for (var i = 0; i < fixedArgCount; i++)
            {
                sig.FixedArgs[i] = ParseEncodedArg(reader, constructorSig.Parameters[i].Type);
            }

            var namedArgCount = reader.ReadRawInt16();
            sig.NamedArgs = new NamedArg[namedArgCount];
            for (var i = 0; i < namedArgCount; i++)
            {
                sig.NamedArgs[i] = ParseNamedEncodedArg(reader);
            }

            return sig;
        }

        private FixedArg ParseEncodedArg(SignatureReader reader, TypeSig typeSig)
        {
            if (typeSig is SzArrayTypeSig szArrayTypeSig)
            {
                var elementType = GetSimpleType(szArrayTypeSig.ElementType);

                var count = reader.ReadRawUInt32();
                if (count == uint.MaxValue) return new FixedArg { Type = elementType.MakeArrayType(), Value = null };
                var values = Array.CreateInstance(elementType, count);
                for (var i = 0; i < count; i++)
                {
                    var value = ParseElement(reader, elementType);
                    values.SetValue(value, i);
                }
                return new FixedArg { Type = elementType.MakeArrayType(), Value = values };
            }
            if (typeSig is SimpleTypeSig simpleType)
            {
                var type = GetSimpleType(simpleType);
                return ParseElement(reader, type);
            }
            if (typeSig is EnumTypeSig enumType)
            {
                var actualUnderlyingType = enumTypeResolver.GetUnderlyingType(enumType);
                var type = GetSimpleType(actualUnderlyingType);
                return ParseElement(reader, type);
            }
            throw new InvalidOperationException($"Unable to parse encoded argument of type {typeSig}.");
        }

        private FixedArg ParseElement(SignatureReader reader, Type type)
        {
            if (type == typeof(object))
            {
                var actualType = new SignatureParser().ParseType(reader);
                if (actualType is SimpleTypeSig simpleType)
                {
                    return ParseSimpleElement(reader, simpleType.SimpleType);
                }
                throw new InvalidOperationException($"{actualType} is not a simple type.");
            }
            return ParseSimpleElement(reader, type);
        }

        private FixedArg ParseSimpleElement(SignatureReader reader, Type type)
        {
            return new FixedArg { Value = ReadSimpleElementValue(reader, type), Type = type };
        }

        private object ReadSimpleElementValue(SignatureReader reader, Type type)
        {
            if (type == typeof(string)) return reader.ReadPackedString();
            if (type == typeof(bool)) return reader.ReadRawBoolean();
            if (type == typeof(char)) return reader.ReadRawCharacter();
            if (type == typeof(sbyte)) return (sbyte)reader.ReadRawByte();
            if (type == typeof(byte)) return reader.ReadRawByte();
            if (type == typeof(short)) return reader.ReadBytesAs(BitConverter.ToInt16, 2);
            if (type == typeof(ushort)) return reader.ReadBytesAs(BitConverter.ToUInt16, 2);
            if (type == typeof(int)) return reader.ReadBytesAs(BitConverter.ToInt32, 4);
            if (type == typeof(uint)) return reader.ReadBytesAs(BitConverter.ToUInt32, 4);
            if (type == typeof(long)) return reader.ReadBytesAs(BitConverter.ToInt64, 8);
            if (type == typeof(ulong)) return reader.ReadBytesAs(BitConverter.ToUInt64, 8);
            if (type == typeof(float)) return reader.ReadBytesAs(BitConverter.ToSingle, 4);
            if (type == typeof(double)) return reader.ReadBytesAs(BitConverter.ToDouble, 8);
            throw new InvalidOperationException($"{type} is not a simple type.");
        }

        private NamedArg ParseNamedEncodedArg(SignatureReader reader)
        {
            var memberKind = reader.ReadElementType();
            Debug.Assert(memberKind == ElementType.CAttr_Field || memberKind == ElementType.CAttr_Prop);

            if (reader.TryMatchElementType(ElementType.CAttr_Boxed))
            {
                var memberName = reader.ReadPackedString();
                var memberType = new SignatureParser().ParseType(reader);
                var arg = ParseEncodedArg(reader, memberType);
                return new NamedArg {
                    IsProperty = memberKind == ElementType.CAttr_Prop,
                    Name = memberName,
                    Type = arg.Type,
                    Value = arg.Value
                };
            }
            else
            {
                var memberType = new SignatureParser().ParseType(reader);
                var memberName = reader.ReadPackedString();
                var arg = ParseEncodedArg(reader, memberType);
                return new NamedArg {
                    IsProperty = memberKind == ElementType.CAttr_Prop,
                    Name = memberName,
                    Type = arg.Type,
                    Value = arg.Value
                };
            }
        }

        private Type GetSimpleType(TypeSig typeSig)
        {
            if (typeSig == null) throw new ArgumentNullException(nameof(typeSig));
            if (typeSig is SzArrayTypeSig szArrayType)
            {
                var elementType = GetSimpleType(szArrayType.ElementType);
                return elementType.MakeArrayType();
            }
            if (typeSig is SimpleTypeSig simpleType)
            {
                // Types are recorded as strings (full names) in attributes.
                if (simpleType.SimpleType == typeof(Type)) return typeof(string);
                return simpleType.SimpleType;
            }
            throw new InvalidOperationException($"{typeSig.GetType()} is not a simple type.");
        }
    }
}

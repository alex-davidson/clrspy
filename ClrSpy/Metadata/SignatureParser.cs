using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClrSpy.Metadata
{
    public class SignatureParser
    {
        public object ParseSignature(SignatureReader reader)
        {
            switch (reader.PeekSignatureType())
            {
                case SignatureType.Default:
                case SignatureType.C:
                case SignatureType.StdCall:
                case SignatureType.ThisCall:
                case SignatureType.FastCall:
                case SignatureType.VarArg:
                    return ParseMethod(reader);

                case SignatureType.Field:
                    return ParseField(reader);

                case SignatureType.Property:
                    return ParseProperty(reader);

                default:
                    throw new SignatureFormatException($"Unknown signature type: {reader.PeekSignatureType()}", reader.Offset);
            }
        }

        public PropertySig ParseProperty(SignatureReader reader)
        {
            var sig = new PropertySig { CallingConvention = reader.ReadCallingConvention() };
            var paramCount = reader.ReadUInt32();
            sig.CustomModifiers = ParseCustomModifiers(reader);
            sig.Type = ParseType(reader);
            sig.Parameters = new ParamSig[paramCount];
            for (var i = 0; i < paramCount; i++)
            {
                sig.Parameters[i] = ParseParam(reader);
            }
            return sig;
        }

        public FieldSig ParseField(SignatureReader reader)
        {
            var sig = new FieldSig();
            sig.CustomModifiers = ParseCustomModifiers(reader);
            sig.Type = ParseType(reader);
            return sig;
        }

        public MethodSig ParseMethod(SignatureReader reader)
        {
            var sig = new MethodSig { CallingConvention = reader.ReadCallingConvention() };
            if (sig.CallingConvention.HasFlag(CallingConvention.Generic))
            {
                sig.GenericParameterCount = (int)reader.ReadUInt32();
            }
            var paramCount = reader.ReadUInt32();
            sig.ReturnType = ParseType(reader);
            sig.Parameters = new ParamSig[paramCount];
            for (var i = 0; i < paramCount; i++)
            {
                sig.Parameters[i] = ParseParam(reader);
            }
            return sig;
        }

        private ParamSig ParseParam(SignatureReader reader)
        {
            var parameter = new ParamSig();
            parameter.CustomModifiers = ParseCustomModifiers(reader);
            if (reader.TryMatchElementType(ElementType.TypedByRef))
            {
                parameter.IsTypedByRef = true;
                return parameter;
            }
            parameter.IsByRef = reader.TryMatchElementType(ElementType.ByRef);
            parameter.Type = ParseType(reader);
            if (parameter.Type is SimpleTypeSig type)
            {
                Debug.Assert(type.SimpleType != typeof(void));
            }
            return parameter;
        }

        private MetadataToken[] ParseCustomModifiers(SignatureReader reader)
        {
            if (!reader.TryReadCustomModifier(out var token)) return null;
            var customModifiers = new List<MetadataToken> { token };
            while (reader.TryReadCustomModifier(out token)) customModifiers.Add(token);
            return customModifiers.ToArray();
        }

        public static Type MapToSimpleType(ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Boolean:   return typeof(bool);
                case ElementType.Char:      return typeof(char);
                case ElementType.I1:        return typeof(sbyte);
                case ElementType.U1:        return typeof(byte);
                case ElementType.I2:        return typeof(short);
                case ElementType.U2:        return typeof(ushort);
                case ElementType.I4:        return typeof(int);
                case ElementType.U4:        return typeof(uint);
                case ElementType.I8:        return typeof(long);
                case ElementType.U8:        return typeof(ulong);
                case ElementType.R4:        return typeof(float);
                case ElementType.R8:        return typeof(double);
                case ElementType.I:         return typeof(IntPtr);
                case ElementType.U:         return typeof(UIntPtr);
                case ElementType.String:    return typeof(string);
                case ElementType.Object:    return typeof(object);
                case ElementType.Void:      return typeof(void);
                case ElementType.Type:      return typeof(Type);
            }
            throw new ArgumentException($"Not a simple type: {elementType}");
        }

        public TypeSig ParseType(SignatureReader reader)
        {
            var elementType = reader.ReadElementType();
            switch (elementType)
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                case ElementType.I:
                case ElementType.U:
                case ElementType.String:
                case ElementType.Object:
                case ElementType.Void:
                case ElementType.Type:
                    return new SimpleTypeSig { SimpleType = MapToSimpleType(elementType) };
                case ElementType.Ptr: {
                        var sig = new PointerTypeSig();
                        sig.CustomModifiers = ParseCustomModifiers(reader);
                        sig.PointerType = ParseType(reader);
                        return sig;
                    }
                case ElementType.ValueType:
                    return new ComplexTypeSig { MetadataToken = reader.ReadEncodedTypeDefOrRefOrSpec(), IsValueType = true };
                case ElementType.Class:
                    return new ComplexTypeSig { MetadataToken = reader.ReadEncodedTypeDefOrRefOrSpec() };
                case ElementType.Var:
                    return new GenericArgumentTypeSig { Index = (int)reader.ReadUInt32() };
                case ElementType.Array:         return ParseArrayType(reader);
                case ElementType.GenericInst:   return ParseGenericType(reader);
                case ElementType.FnPtr:
                    return new FunctionPointerTypeSig { Function = ParseMethod(reader) };
                case ElementType.SzArray: {
                        var sig = new SzArrayTypeSig();
                        sig.CustomModifiers = ParseCustomModifiers(reader);
                        sig.ElementType = ParseType(reader);
                        return sig;
                    }
                case ElementType.MVar:
                    return new GenericArgumentTypeSig { Index = (int)reader.ReadUInt32(), IsFromMethod = true };

                case ElementType.CAttr_Enum:
                    return new EnumTypeSig { TypeName = reader.ReadPackedString() };

                case ElementType.End:
                case ElementType.ByRef:
                case ElementType.TypedByRef:
                case ElementType.CMod_Reqd:
                case ElementType.CMod_Opt:
                case ElementType.Internal:
                case ElementType.Modifier:
                case ElementType.Sentinel:
                case ElementType.Pinned:
                case ElementType.Reserved:
                case ElementType.CAttr_Field:
                case ElementType.CAttr_Prop:
                case ElementType.CAttr_Boxed:
                    throw new SignatureFormatException($"Unexpected ElementType: {elementType}", reader.Offset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ArrayTypeSig ParseArrayType(SignatureReader reader)
        {
            var sig = new ArrayTypeSig();
            sig.ElementType = ParseType(reader);
            var rank = reader.ReadUInt32();
            Debug.Assert(rank >= 1);
            sig.Dimensions = new ArrayDimension[rank];
            var numSizes = reader.ReadUInt32();
            for (var i = 0; i < numSizes; i++)
            {
                sig.Dimensions[i].Size = (int)reader.ReadUInt32();
            }
            var numLowerBounds = reader.ReadUInt32();
            for (var i = 0; i < numLowerBounds; i++)
            {
                sig.Dimensions[i].LowerBound = reader.ReadInt32();
            }
            return sig;
        }

        private GenericTypeSig ParseGenericType(SignatureReader reader)
        {
            var sig = new GenericTypeSig();
            var typeDefType = reader.ReadElementType();
            Debug.Assert(typeDefType == ElementType.Class || typeDefType == ElementType.ValueType);
            sig.TypeDefinition = new ComplexTypeSig {
                MetadataToken = reader.ReadEncodedTypeDefOrRefOrSpec(),
                IsValueType = typeDefType == ElementType.ValueType
            };
            var genericArgumentCount = reader.ReadUInt32();
            sig.GenericArguments = new TypeSig[genericArgumentCount];
            for (var i = 0; i < genericArgumentCount; i++)
            {
                sig.GenericArguments[i] = ParseType(reader);
            }
            return sig;
        }
    }
}

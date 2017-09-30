using System;

namespace ClrSpy.Metadata
{
    public abstract class TypeSig
    {
    }

    public class SimpleTypeSig : TypeSig
    {
        public Type SimpleType { get; set; }
    }

    public class PointerTypeSig : TypeSig
    {
        public MetadataToken[] CustomModifiers { get; set; }
        public TypeSig PointerType { get; set; }
    }

    public class ComplexTypeSig : TypeSig
    {
        public bool IsValueType { get; set; }
        public MetadataToken MetadataToken { get; set; }
    }

    public class GenericArgumentTypeSig : TypeSig
    {
        public bool IsFromMethod { get; set; }
        public int Index { get; set; }
    }

    public class ArrayTypeSig : TypeSig
    {
        public ArrayDimension[] Dimensions { get; set; }
        public TypeSig ElementType { get; set; }
    }

    public class SzArrayTypeSig : TypeSig
    {
        public MetadataToken[] CustomModifiers { get; set; }
        public TypeSig ElementType { get; set; }
    }

    public class GenericTypeSig : TypeSig
    {
        public ComplexTypeSig TypeDefinition { get; set; }
        public TypeSig[] GenericArguments { get; set; }
    }

    public class EnumTypeSig : TypeSig
    {
        public string TypeName { get; set; }
    }

    public class FunctionPointerTypeSig : TypeSig
    {
        public MethodSig Function { get; set; }
    }

    public struct ArrayDimension
    {
        public int? Size { get; set; }
        public int LowerBound { get; set; }
        public int? UpperBound => Size + LowerBound;
    }
}

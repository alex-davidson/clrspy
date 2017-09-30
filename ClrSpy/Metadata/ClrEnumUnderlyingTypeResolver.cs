using System;

namespace ClrSpy.Metadata
{
    public class ClrEnumUnderlyingTypeResolver
    {
        private readonly MetadataImportWrapper mdImport;

        public ClrEnumUnderlyingTypeResolver(MetadataImportWrapper mdImport)
        {
            this.mdImport = mdImport;
        }

        public SimpleTypeSig GetUnderlyingType(EnumTypeSig enumType)
        {
            var typeDef = mdImport.FindTypeDefByName(enumType.TypeName);
            if (typeDef != MetadataToken.Null)
            {
                var underlyingTypeSig = mdImport.GetEnumUnderlyingType(typeDef);
                // Should be a primitive type of some kind.
                if (underlyingTypeSig is SimpleTypeSig simpleType)
                {
                    return simpleType;
                }
                throw new NotSupportedException($"Can't resolve base type for enum {enumType.TypeName}");
            }
            else
            {
                var actualEnumType = Type.GetType(enumType.TypeName);
                if (actualEnumType == null) throw new NotSupportedException($"Can't resolve base type for enum {enumType.TypeName}");
                return new SimpleTypeSig { SimpleType = actualEnumType.GetEnumUnderlyingType() };
            }
        }
    }
}

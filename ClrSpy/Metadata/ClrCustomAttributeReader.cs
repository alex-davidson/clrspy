using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Metadata
{
    public class ClrCustomAttributeReader
    {
        public ClrCustomAttribute[] GetAttributes(ClrType type)
        {
            var mdImport = type.Module.MetadataImport as IMetadataImport;
            return GetAttributes(new MetadataImportWrapper(mdImport), new MetadataToken(type.MetadataToken)).ToArray();
        }
        
        public ClrCustomAttribute[] GetAttributes(ClrMethod method)
        {
            var mdImport = method.Type.Module.MetadataImport as IMetadataImport;
            return GetAttributes(new MetadataImportWrapper(mdImport), new MetadataToken(method.MetadataToken)).ToArray();
        }

        private IEnumerable<ClrCustomAttribute> GetAttributes(MetadataImportWrapper mdImport, MetadataToken target)
        {
            var attributeTokens = mdImport.EnumCustomAttributes(target);
            foreach (var attributeToken in attributeTokens)
            {
                mdImport.GetCustomAttributeProps(attributeToken, out var attributeConstructorToken, out var attributeBlob);

                if (GetAttributeConstructorProps(mdImport, attributeConstructorToken, out var attributeType, out var attributeSigBlob))
                {
                    var attributeTypeName = mdImport.GetTypeName(attributeType);

                    var methodReader = new SignatureReader(attributeSigBlob);
                    var attrMethod = new SignatureParser().ParseMethod(methodReader);

                    var attrReader = new SignatureReader(attributeBlob);
                    var attr = new CustomAttributeSignatureParser(new ClrEnumUnderlyingTypeResolver(mdImport)).Parse(attrReader, attrMethod);

                    yield return new ClrCustomAttribute(attributeTypeName, attrMethod, attr);
                }
                else throw new NotImplementedException();
            }
        }

        private static bool GetAttributeConstructorProps(MetadataImportWrapper mdImport, MetadataToken attributeConstructorToken, out MetadataToken attributeType, out byte[] attributeSigBlob)
        {
            if (attributeConstructorToken.Type == MetadataTokenType.mdtMemberRef)
            {
                mdImport.GetMemberRefProps(attributeConstructorToken, out _, out attributeType, out attributeSigBlob);
                return true;
            }
            else if (attributeConstructorToken.Type == MetadataTokenType.mdtMethodDef)
            {
                mdImport.GetMethodProps(attributeConstructorToken, out _, out attributeType, out attributeSigBlob);
                return true;
            }
            attributeType = default(MetadataToken);
            attributeSigBlob = null;
            return false;
        }
    }
}

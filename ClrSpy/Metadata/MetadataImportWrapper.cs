using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ClrSpy.Metadata
{
    public class MetadataImportWrapper
    {
        private readonly IMetadataImport mdImport;

        public MetadataImportWrapper(IMetadataImport mdImport)
        {
            this.mdImport = mdImport;
        }

        public IList<MetadataToken> EnumCustomAttributes(MetadataToken member)
        {
            var enumPtr = IntPtr.Zero;
            var list = new List<MetadataToken>();
            mdImport.EnumCustomAttributes(ref enumPtr, member, 0, out var caToken, 1, out var count);
            try
            {
                while (count > 0)
                {
                    list.Add(caToken);
                    mdImport.EnumCustomAttributes(ref enumPtr, member, 0, out caToken, 1, out count);
                }
            }
            finally
            {
                mdImport.CloseEnum(enumPtr);
            }
            return list;
        }

        public void GetCustomAttributeProps(MetadataToken attributeToken, out MetadataToken attributeConstructorToken, out byte[] attributeBlob)
        {
            mdImport.GetCustomAttributeProps(attributeToken, out var targetToken, out attributeConstructorToken, out var ppBlob, out var pcbSize);
            attributeBlob = new byte[pcbSize];
            Marshal.Copy(ppBlob, attributeBlob, 0, (int) pcbSize);
        }

        public void GetMemberRefProps(MetadataToken memberToken, out string memberName, out MetadataToken declaringType, out byte[] signatureBlob)
        {
            var nameBuffer = new StringBuilder(512);
            mdImport.GetMemberRefProps(memberToken, out declaringType, nameBuffer, nameBuffer.Capacity, out var actualNameLength, out var sigBlob, out var sigBlobSize);
            Debug.Assert(actualNameLength <= nameBuffer.Capacity);
            signatureBlob = new byte[sigBlobSize];
            Marshal.Copy(sigBlob, signatureBlob, 0, sigBlobSize);
            memberName = nameBuffer.ToString();
        }

        public void GetMethodProps(MetadataToken methodToken, out string methodName, out MetadataToken declaringType, out byte[] signatureBlob)
        {
            var nameBuffer = new StringBuilder(512);
            mdImport.GetMethodProps(methodToken, out declaringType, nameBuffer, nameBuffer.Capacity, out var actualNameLength, out var pdwAttr, out var sigBlob, out var sigBlobSize, out var pulCodeRVA, out var pdwImplFlags);
            Debug.Assert(actualNameLength <= nameBuffer.Capacity);
            signatureBlob = new byte[sigBlobSize];
            Marshal.Copy(sigBlob, signatureBlob, 0, sigBlobSize);
            methodName = nameBuffer.ToString();
        }

        public string GetTypeName(MetadataToken typeToken)
        {
            if(typeToken.Type == MetadataTokenType.mdtTypeDef)
            {
                GetTypeDefProps(typeToken, out var typeName);
                return typeName;
            }
            if(typeToken.Type == MetadataTokenType.mdtTypeRef)
            {
                GetTypeRefProps(typeToken, out var typeName);
                return typeName;
            }
            throw new NotSupportedException(typeToken.Type.ToString());
        }

        public void GetTypeRefProps(MetadataToken typeToken, out string typeName)
        {
            var nameBuffer = new StringBuilder(512);
            mdImport.GetTypeRefProps(typeToken, out var ptkScope, nameBuffer, nameBuffer.Capacity, out var actualNameLength);
            Debug.Assert(actualNameLength <= nameBuffer.Capacity);
            typeName = nameBuffer.ToString();
        }

        public void GetTypeDefProps(MetadataToken typeToken, out string typeName)
        {
            var nameBuffer = new StringBuilder(512);
            mdImport.GetTypeDefProps(typeToken, nameBuffer, nameBuffer.Capacity, out var actualNameLength, out var pdwTypeDefFlags, out var ptkExtends);
            Debug.Assert(actualNameLength <= nameBuffer.Capacity);
            typeName = nameBuffer.ToString();
        }

        public MetadataToken FindTypeDefByName(string typeName)
        {
            var result = mdImport.FindTypeDefByName(typeName, MetadataToken.Null, out var typeDef); 
            if (result != 0) return MetadataToken.Null;
            return typeDef;
        }

        public TypeSig GetEnumUnderlyingType(MetadataToken enumType)
        {
            var enumPtr = IntPtr.Zero;
            var matches = new uint[1];
            mdImport.EnumFields(ref enumPtr, enumType, matches, matches.Length, out var count);
            try
            {
                while (count > 0)
                {
                    var nameBuffer = new StringBuilder(512);
                    mdImport.GetFieldProps((int)matches[0], out var type, nameBuffer, nameBuffer.Capacity, out var actualNameLength, out var pdwAttr, out var sigBlob, out var sigBlobSize, out var pdwCPlusTypeFlab, out var ppValue, out var pcchValue);
                    if (nameBuffer.ToString() == "value__")
                    {
                        var signatureBlob = new byte[sigBlobSize];
                        Marshal.Copy(sigBlob, signatureBlob, 0, sigBlobSize);
                        var sig = new SignatureParser().ParseField(new SignatureReader(signatureBlob));
                        return sig.Type;
                    }
                }
            }
            finally
            {
                mdImport.CloseEnum(enumPtr);
            }
            return null;
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace ClrSpy.Metadata
{
    // From https://github.com/dotnet/coreclr/blob/master/src/inc/cor.h , plus some guidance from ClrMD
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    public interface IMetadataImport
    {
        //STDMETHOD_(void, CloseEnum)(HCORENUM hEnum) PURE;
        [PreserveSig]
        void CloseEnum(IntPtr hEnum);

        //STDMETHOD(CountEnum)(HCORENUM hEnum, ULONG *pulCount) PURE;
        void CountEnum(IntPtr hEnum, [ComAliasName("ULONG*")] out int pulCount);

        //STDMETHOD(ResetEnum)(HCORENUM hEnum, ULONG ulPos) PURE;
        void ResetEnum(IntPtr hEnum, int ulPos);

        void EnumTypeDefs_();
        void EnumInterfaceImpls_();

        //STDMETHOD(EnumTypeRefs)(HCORENUM *phEnum, mdTypeRef rTypeRefs[], ULONG cMax, ULONG* pcTypeRefs) PURE;
        void EnumTypeRefs_();

        //     STDMETHOD(FindTypeDefByName)(           // S_OK or error.
        //         LPCWSTR     szTypeDef,              // [IN] Name of the Type.
        //         mdToken     tkEnclosingClass,       // [IN] TypeDef/TypeRef for Enclosing class.
        //         mdTypeDef   *ptd) PURE;             // [OUT] Put the TypeDef token here.
        [PreserveSig]
        int FindTypeDefByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szTypeDef,
            [In] MetadataToken tkEnclosingClass,
            [ComAliasName("mdTypeDef*")] [Out] out MetadataToken token
        );

        void GetScopeProps_();
        void GetModuleFromScope_();

        //     STDMETHOD(GetTypeDefProps)(             // S_OK or error.
        //         mdTypeDef   td,                     // [IN] TypeDef token for inquiry.
        //         LPWSTR      szTypeDef,              // [OUT] Put name here.
        //         ULONG       cchTypeDef,             // [IN] size of name buffer in wide chars.
        //         ULONG       *pchTypeDef,            // [OUT] put size of name (wide chars) here.
        //         DWORD       *pdwTypeDefFlags,       // [OUT] Put flags here.
        //         mdToken     *ptkExtends) PURE;      // [OUT] Put base class TypeDef/TypeRef here.
        [PreserveSig]
        int GetTypeDefProps([In] int td,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szTypeDef,
            [In] int cchTypeDef,
            [ComAliasName("ULONG*")] [Out] out int pchTypeDef,
            [Out, MarshalAs(UnmanagedType.U4)] out System.Reflection.TypeAttributes pdwTypeDefFlags,
            [ComAliasName("mdToken*")] [Out] out MetadataToken ptkExtends
        );

        void GetInterfaceImplProps_();

        //     STDMETHOD(GetTypeRefProps)(             // S_OK or error.
        //         mdTypeRef   tr,                     // [IN] TypeRef token.
        //         mdToken     *ptkResolutionScope,    // [OUT] Resolution scope, ModuleRef or AssemblyRef.
        //         LPWSTR      szName,                 // [OUT] Name of the TypeRef.
        //         ULONG       cchName,                // [IN] Size of buffer.
        //         ULONG       *pchName) PURE;         // [OUT] Size of Name.

        [PreserveSig]
        int GetTypeRefProps(
            int tr,
            [ComAliasName("mdToken*")] [Out] out MetadataToken ptkResolutionScope,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName,
            [In] int cchName,
            [ComAliasName("ULONG*")] out int pchName
        );

        void ResolveTypeRef_();
        void EnumMembers_();
        void EnumMembersWithName_();
        void EnumMethods_();
        void EnumMethodsWithName_();

        //     STDMETHOD(EnumFields)(                 // S_OK, S_FALSE, or error.  
        //         HCORENUM    *phEnum,                // [IN|OUT] Pointer to the enum.    
        //         mdTypeDef   cl,                     // [IN] TypeDef to scope the enumeration.   
        //         mdFieldDef  rFields[],              // [OUT] Put FieldDefs here.    
        //         ULONG       cMax,                   // [IN] Max FieldDefs to put.   
        //         ULONG       *pcTokens) PURE;        // [OUT] Put # put here.    
        [PreserveSig]
        int EnumFields(ref IntPtr phEnum,
            int cl,
            [ComAliasName("mdFieldDef*")] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] mdFieldDef,
            int cMax,
            [ComAliasName("ULONG*")] out int pcTokens
        );

        void EnumFieldsWithName_();
        void EnumParams_();
        void EnumMemberRefs_();
        void EnumMethodImpls_();
        void EnumPermissionSets_();
        void FindMember_();
        void FindMethod_();
        void FindField_();
        void FindMemberRef_();

        //     STDMETHOD (GetMethodProps)( 
        //         mdMethodDef mb,                     // The method for which to get props.   
        //         mdTypeDef   *pClass,                // Put method's class here. 
        //         LPWSTR      szMethod,               // Put method's name here.  
        //         ULONG       cchMethod,              // Size of szMethod buffer in wide chars.   
        //         ULONG       *pchMethod,             // Put actual size here 
        //         DWORD       *pdwAttr,               // Put flags here.  
        //         PCCOR_SIGNATURE *ppvSigBlob,        // [OUT] point to the blob value of meta data   
        //         ULONG       *pcbSigBlob,            // [OUT] actual size of signature blob  
        //         ULONG       *pulCodeRVA,            // [OUT] codeRVA    
        //         DWORD       *pdwImplFlags) PURE;    // [OUT] Impl. Flags    
        [PreserveSig]
        int GetMethodProps(
            [In] uint md,
            [ComAliasName("mdTypeDef*")] [Out] out MetadataToken pClass,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szMethod,
            [In] int cchMethod,
            [ComAliasName("ULONG*")] [Out] out int pchMethod,
            [ComAliasName("DWORD*")] [Out] out System.Reflection.MethodAttributes pdwAttr,
            [ComAliasName("PCCOR_SIGNATURE*")] [Out] out IntPtr ppvSigBlob,
            [ComAliasName("ULONG*")] [Out] out int pcbSigBlob,
            [ComAliasName("ULONG*")] [Out] out uint pulCodeRVA,
            [ComAliasName("DWORD*")] [Out] out uint pdwImplFlags
        );

        //     STDMETHOD(GetMemberRefProps)(           // S_OK or error.   
        //         mdMemberRef mr,                     // [IN] given memberref 
        //         mdToken     *ptk,                   // [OUT] Put classref or classdef here. 
        //         LPWSTR      szMember,               // [OUT] buffer to fill for member's name   
        //         ULONG       cchMember,              // [IN] the count of char of szMember   
        //         ULONG       *pchMember,             // [OUT] actual count of char in member name    
        //         PCCOR_SIGNATURE *ppvSigBlob,        // [OUT] point to meta data blob value  
        //         ULONG       *pbSig) PURE;           // [OUT] actual size of signature blob  
        void GetMemberRefProps(
            [In] uint mr,
            [ComAliasName("mdMemberRef*")] [Out] out MetadataToken ptk,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szMember,
            [In] int cchMember,
            [ComAliasName("ULONG*")] [Out] out uint pchMember,
            [ComAliasName("PCCOR_SIGNATURE*")] [Out] out IntPtr ppvSigBlob,
            [ComAliasName("ULONG*")] [Out] out int pbSig
        );

        void EnumProperties_();
        void EnumEvents_();
        void GetEventProps_();
        void EnumMethodSemantics_();
        void GetMethodSemantics_();
        void GetClassLayout_();
        void GetFieldMarshal_();

        //     STDMETHOD(GetRVA)(                      // S_OK or error.   
        //         mdToken     tk,                     // Member for which to set offset   
        //         ULONG       *pulCodeRVA,            // The offset   
        //         DWORD       *pdwImplFlags) PURE;    // the implementation flags 
        [PreserveSig]
        int GetRVA(uint token, [Out] out uint pRva, [Out] out uint flags);

        void GetPermissionSetProps_();
        void GetSigFromToken_();
        void GetModuleRefProps_();
        void EnumModuleRefs_();
        void GetTypeSpecFromToken_();
        void GetNameFromToken_();
        void EnumUnresolvedMethods_();
        void GetUserString_();
        void GetPinvokeMap_();
        void EnumSignatures_();
        void EnumTypeSpecs_();
        void EnumUserStrings_();
        void GetParamForMethodIndex_();

        //     STDMETHOD(EnumCustomAttributes)(        // S_OK or error.
        //         HCORENUM    *phEnum,                // [IN, OUT] COR enumerator.
        //         mdToken     tk,                     // [IN] Token to scope the enumeration, 0 for all.
        //         mdToken     tkType,                 // [IN] Type of interest, 0 for all.
        //         mdCustomAttribute rCustomAttributes[], // [OUT] Put custom attribute tokens here.
        //         ULONG       cMax,                   // [IN] Size of rCustomAttributes.
        //         ULONG       *pcCustomAttributes) PURE;  // [OUT, OPTIONAL] Put count of token values here.
        void EnumCustomAttributes(ref IntPtr phEnum,
                         int tk,
                         int tkType,
                         [ComAliasName("mdCustomAttribute*")]out MetadataToken mdCustomAttribute,
                         uint cMax /*must be 1*/,
                         [ComAliasName("ULONG*")]out uint pcTokens
                         );

        //     STDMETHOD(GetCustomAttributeProps)(     // S_OK or error.
        //         mdCustomAttribute cv,               // [IN] CustomAttribute token.
        //         mdToken     *ptkObj,                // [OUT, OPTIONAL] Put object token here.
        //         mdToken     *ptkType,               // [OUT, OPTIONAL] Put AttrType token here.
        //         void const  **ppBlob,               // [OUT, OPTIONAL] Put pointer to data here.
        //         ULONG       *pcbSize) PURE;         // [OUT, OPTIONAL] Put size of date here.
        void GetCustomAttributeProps(
            int cv,
            [ComAliasName("mdToken*")] [Out] out MetadataToken ptkObj,
            [ComAliasName("mdToken*")] [Out] out MetadataToken ptkType,
            [ComAliasName("UVCP_CONSTANT*")] out IntPtr ppBlob,
            [ComAliasName("ULONG*")] out uint pcbSize);

        void FindTypeRef_();
        void GetMemberProps_();

        //     STDMETHOD(GetFieldProps)(  
        //         mdFieldDef  mb,                     // The field for which to get props.    
        //         mdTypeDef   *pClass,                // Put field's class here.  
        //         LPWSTR      szField,                // Put field's name here.   
        //         ULONG       cchField,               // Size of szField buffer in wide chars.    
        //         ULONG       *pchField,              // Put actual size here 
        //         DWORD       *pdwAttr,               // Put flags here.  
        //         PCCOR_SIGNATURE *ppvSigBlob,        // [OUT] point to the blob value of meta data   
        //         ULONG       *pcbSigBlob,            // [OUT] actual size of signature blob  
        //         DWORD       *pdwCPlusTypeFlag,      // [OUT] flag for value type. selected ELEMENT_TYPE_*   
        //         void const  **ppValue,              // [OUT] constant value 
        //         ULONG       *pcchValue) PURE;       // [OUT] size of constant string in chars, 0 for non-strings.
        [PreserveSig]
        int GetFieldProps(int mb,
            [ComAliasName("mdTypeDef*")] out int mdTypeDef,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szField,
            int cchField,
            [ComAliasName("ULONG*")] out int pchField,
            [ComAliasName("DWORD*")] out System.Reflection.FieldAttributes pdwAttr,
            [ComAliasName("PCCOR_SIGNATURE*")] out IntPtr ppvSigBlob,
            [ComAliasName("ULONG*")] out int pcbSigBlob,
            [ComAliasName("DWORD*")] out int pdwCPlusTypeFlab,
            [ComAliasName("UVCP_CONSTANT*")] out IntPtr ppValue,
            [ComAliasName("ULONG*")] out int pcchValue
        );

        void GetPropertyProps_();
        void GetParamProps_();
        void GetCustomAttributeByName_();

        //     STDMETHOD_(BOOL, IsValidToken)(         // True or False.
        //         mdToken     tk) PURE;               // [IN] Given token.
        [PreserveSig]
        bool IsValidToken([In, MarshalAs(UnmanagedType.U4)] uint tk);

        void GetNestedClassProps_();
        void GetNativeCallConvFromSig_();
        void IsGlobal_();
    }

    [Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMetadataAssemblyImport
    {
        //     STDMETHOD(GetAssemblyProps)(            // S_OK or error.
        //         mdAssembly  mda,                    // [IN] The Assembly for which to get the properties.
        //         const void  **ppbPublicKey,         // [OUT] Pointer to the public key.
        //         ULONG       *pcbPublicKey,          // [OUT] Count of bytes in the public key.
        //         ULONG       *pulHashAlgId,          // [OUT] Hash Algorithm.
        //         LPWSTR      szName,                 // [OUT] Buffer to fill with assembly's simply name.
        //         ULONG       cchName,                // [IN] Size of buffer in wide chars.
        //         ULONG       *pchName,               // [OUT] Actual # of wide chars in name.
        //         ASSEMBLYMETADATA *pMetaData,        // [OUT] Assembly MetaData.
        //         DWORD       *pdwAssemblyFlags) PURE;    // [OUT] Flags.
        void GetAssemblyProps(
            [ComAliasName("mdAssembly")] uint mda,
            [ComAliasName("UVCP_CONSTANT*")] [Out] out IntPtr ppbPublicKey,
            [ComAliasName("ULONG*")] [Out] out uint pcbPublicKey,
            [ComAliasName("ULONG*")] [Out] out uint pulHashAlgId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName,
            [In] int cchName,
            [ComAliasName("ULONG*")] [Out] out int pchName,
            [ComAliasName("ASSEMBLYMETADATA*")] [Out] out IntPtr pMetaData,
            [Out] out uint pdwTypeDefFlags
        );
        //     STDMETHOD(GetAssemblyRefProps)(         // S_OK or error.
        //         mdAssemblyRef mdar,                 // [IN] The AssemblyRef for which to get the properties.
        //         const void  **ppbPublicKeyOrToken,  // [OUT] Pointer to the public key or token.
        //         ULONG       *pcbPublicKeyOrToken,   // [OUT] Count of bytes in the public key or token.
        //         LPWSTR      szName,                 // [OUT] Buffer to fill with name.
        //         ULONG       cchName,                // [IN] Size of buffer in wide chars.
        //         ULONG       *pchName,               // [OUT] Actual # of wide chars in name.
        //         ASSEMBLYMETADATA *pMetaData,        // [OUT] Assembly MetaData.
        //         const void  **ppbHashValue,         // [OUT] Hash blob.
        //         ULONG       *pcbHashValue,          // [OUT] Count of bytes in the hash blob.
        //         DWORD       *pdwAssemblyRefFlags) PURE; // [OUT] Flags.
        void GetAssemblyRefProps(
            [ComAliasName("mdAssemblyRef")] uint mdAssemblyRef,
            [ComAliasName("UVCP_CONSTANT*")] [Out] out IntPtr ppbPublicKeyOrToken,
            [ComAliasName("ULONG*")] [Out] out uint pcbPublicKeyOrToken,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName,
            [In] int cchName,
            [ComAliasName("ULONG*")] [Out] out int pchName,
            [ComAliasName("ASSEMBLYMETADATA*")] [Out] out IntPtr pMetaData,
            [ComAliasName("UVCP_CONSTANT*")] [Out] out IntPtr ppbHashValue,
            [ComAliasName("ULONG*")] [Out] out uint pcbHashValue,
            [Out] out uint pdwTypeDefFlags
        );

        //     STDMETHOD(GetFileProps)(                // S_OK or error.
        //         mdFile      mdf,                    // [IN] The File for which to get the properties.
        //         LPWSTR      szName,                 // [OUT] Buffer to fill with name.
        //         ULONG       cchName,                // [IN] Size of buffer in wide chars.
        //         ULONG       *pchName,               // [OUT] Actual # of wide chars in name.
        //         const void  **ppbHashValue,         // [OUT] Pointer to the Hash Value Blob.
        //         ULONG       *pcbHashValue,          // [OUT] Count of bytes in the Hash Value Blob.
        //         DWORD       *pdwFileFlags) PURE;    // [OUT] Flags.
        void GetFileProps_();

        //     STDMETHOD(GetExportedTypeProps)(        // S_OK or error.
        //         mdExportedType   mdct,              // [IN] The ExportedType for which to get the properties.
        //         LPWSTR      szName,                 // [OUT] Buffer to fill with name.
        //         ULONG       cchName,                // [IN] Size of buffer in wide chars.
        //         ULONG       *pchName,               // [OUT] Actual # of wide chars in name.
        //         mdToken     *ptkImplementation,     // [OUT] mdFile or mdAssemblyRef or mdExportedType.
        //         mdTypeDef   *ptkTypeDef,            // [OUT] TypeDef token within the file.
        //         DWORD       *pdwExportedTypeFlags) PURE; // [OUT] Flags.
        void GetExportedTypeProps_();

        //     STDMETHOD(GetManifestResourceProps)(    // S_OK or error.
        //         mdManifestResource  mdmr,           // [IN] The ManifestResource for which to get the properties.
        //         LPWSTR      szName,                 // [OUT] Buffer to fill with name.
        //         ULONG       cchName,                // [IN] Size of buffer in wide chars.
        //         ULONG       *pchName,               // [OUT] Actual # of wide chars in name.
        //         mdToken     *ptkImplementation,     // [OUT] mdFile or mdAssemblyRef that provides the ManifestResource.
        //         DWORD       *pdwOffset,             // [OUT] Offset to the beginning of the resource within the file.
        //         DWORD       *pdwResourceFlags) PURE;// [OUT] Flags.
        void GetManifestResourceProps_();

        //     STDMETHOD(EnumAssemblyRefs)(            // S_OK or error
        //         HCORENUM    *phEnum,                // [IN|OUT] Pointer to the enum.
        //         mdAssemblyRef rAssemblyRefs[],      // [OUT] Put AssemblyRefs here.
        //         ULONG       cMax,                   // [IN] Max AssemblyRefs to put.
        //         ULONG       *pcTokens) PURE;        // [OUT] Put # put here.
        void EnumAssemblyRefs_();

        //     STDMETHOD(EnumFiles)(                   // S_OK or error
        //         HCORENUM    *phEnum,                // [IN|OUT] Pointer to the enum.
        //         mdFile      rFiles[],               // [OUT] Put Files here.
        //         ULONG       cMax,                   // [IN] Max Files to put.
        //         ULONG       *pcTokens) PURE;        // [OUT] Put # put here.
        void EnumFiles_();

        //     STDMETHOD(EnumExportedTypes)(           // S_OK or error
        //         HCORENUM    *phEnum,                // [IN|OUT] Pointer to the enum.
        //         mdExportedType   rExportedTypes[],  // [OUT] Put ExportedTypes here.
        //         ULONG       cMax,                   // [IN] Max ExportedTypes to put.
        //         ULONG       *pcTokens) PURE;        // [OUT] Put # put here.
        void EnumExportedTypes_();

        //     STDMETHOD(EnumManifestResources)(       // S_OK or error
        //         HCORENUM    *phEnum,                // [IN|OUT] Pointer to the enum.
        //         mdManifestResource  rManifestResources[],   // [OUT] Put ManifestResources here.
        //         ULONG       cMax,                   // [IN] Max Resources to put.
        //         ULONG       *pcTokens) PURE;        // [OUT] Put # put here.
        void EnumManifestResources_();

        //     STDMETHOD(GetAssemblyFromScope)(        // S_OK or error
        //         mdAssembly  *ptkAssembly) PURE;     // [OUT] Put token here.
        void GetAssemblyFromScope_();

        //     STDMETHOD(FindExportedTypeByName)(      // S_OK or error
        //         LPCWSTR     szName,                 // [IN] Name of the ExportedType.
        //         mdToken     mdtExportedType,        // [IN] ExportedType for the enclosing class.
        //         mdExportedType   *ptkExportedType) PURE; // [OUT] Put the ExportedType token here.
        void FindExportedTypeByName_();

        //     STDMETHOD(FindManifestResourceByName)(  // S_OK or error
        //         LPCWSTR     szName,                 // [IN] Name of the ManifestResource.
        //         mdManifestResource *ptkManifestResource) PURE;  // [OUT] Put the ManifestResource token here.
        void FindManifestResourceByName_();

        //     STDMETHOD_(void, CloseEnum)(
        //         HCORENUM hEnum) PURE;               // Enum to be closed.
        void CloseEnum([In] IntPtr hEnum);

        //     STDMETHOD(FindAssembliesByName)(        // S_OK or error
        //         LPCWSTR  szAppBase,                 // [IN] optional - can be NULL
        //         LPCWSTR  szPrivateBin,              // [IN] optional - can be NULL
        //         LPCWSTR  szAssemblyName,            // [IN] required - this is the assembly you are requesting
        //         IUnknown *ppIUnk[],                 // [OUT] put IMetaDataAssemblyImport pointers here
        //         ULONG    cMax,                      // [IN] The max number to put
        //         ULONG    *pcAssemblies) PURE;       // [OUT] The number of assemblies returned.
        void FindAssembliesByName_();
    }
}

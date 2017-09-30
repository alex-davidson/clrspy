namespace ClrSpy.Metadata
{
    public enum ElementType
    {
        End = 0x00,         // Marks end of a list
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        I1 = 0x04,
        U1 = 0x05,
        I2 = 0x06,
        U2 = 0x07,
        I4 = 0x08,
        U4 = 0x09,
        I8 = 0x0a,
        U8 = 0x0b,
        R4 = 0x0c,
        R8 = 0x0d,
        String = 0x0e,
        Ptr = 0x0f,         // Followed by type
        ByRef = 0x10,       // Followed by type
        ValueType = 0x11,   // Followed by TypeDef or TypeRef token
        Class = 0x12,       // Followed by TypeDef or TypeRef token
        Var = 0x13,         // Generic parameter in a generic type definition, represented as number (compressed unsigned integer)
        Array = 0x14,       // type rank boundsCount bound1 ... loCount lo1 ...
        GenericInst = 0x15, // Generic type instantiation. Followed by type type-arg-count type-1 ... type-n
        TypedByRef = 0x16,  
        I = 0x18,           // System.IntPtr
        U = 0x19,           // System.UIntPtr
        FnPtr = 0x1b,       // Followed by full method signature
        Object = 0x1c,      // System.Object
        SzArray = 0x1d,     // Single-dim array with 0 lower bound
        MVar = 0x1e,        // Generic parameter in a generic method definition, represented as number (compressed unsigned integer)
        CMod_Reqd = 0x1f,   // Required modifier : followed by a TypeDef or TypeRef token
        CMod_Opt = 0x20,    // Optional modifier : followed by a TypeDef or TypeRef token
        Internal = 0x21,    // Implemented within the CLI
        Modifier = 0x40,    // Or’d with following element types
        Sentinel = 0x41,    // Sentinel for vararg method signature
        Pinned = 0x45,      // Denotes a local variable that points at a pinned object
        Type = 0x50,        // Indicates an argument of type System.Type.
        CAttr_Boxed = 0x51, // Used in custom attributes to specify a boxed object (II.23.3).
        Reserved = 0x52,
        CAttr_Field = 0x53, // Used in custom attributes to indicate a FIELD (II.22.10, II.23.3).
        CAttr_Prop = 0x54,  // Used in custom attributes to indicate a PROPERTY (II.22.10, II.23.3).
        CAttr_Enum = 0x55,  // Used in custom attributes to specify an enum (II.23.3).
    }
}

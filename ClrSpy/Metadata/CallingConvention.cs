using System;

namespace ClrSpy.Metadata
{
    [Flags]
    public enum CallingConvention
    {
        Default  = 0x0,
        VarArg   = 0x5,
        Generic      = 0x10,
        HasThis      = 0x20,
        ExplicitThis = 0x40
    }

    public enum SignatureType
    {
        Default  = 0x0,
        C        = 0x1,
        StdCall  = 0x2,
        ThisCall = 0x3,
        FastCall = 0x4,
        VarArg   = 0x5,
        Field    = 0x6,
        Property = 0x8
    }
}

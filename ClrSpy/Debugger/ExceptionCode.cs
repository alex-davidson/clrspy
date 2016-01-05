namespace ClrSpy.Debugger
{
    public enum ExceptionCode : ulong
    {
        AccessViolation = 0xC0000005,
        ArrayBoundsExceeded = 0xC000008C,
        Breakpoint = 0x80000003,
        DatatypeMisalignment = 0x80000002,
        FloatDenormalOperand = 0xC000008D,
        FloatDivideByZero = 0xC000008E,
        FloatInexactResult = 0xC000008F,
        FloatInvalidOperation = 0xC0000090,
        FloatOverflow = 0xC0000091,
        FloatStackCheck = 0xC0000092,
        FloatUnderflow = 0xC0000093,
        IllegalInstruction = 0xC000001D,
        InPageError = 0xC0000006,
        IntegerDivideByZero = 0xC0000094,
        IntegerOverflow = 0xC0000095,
        InvalidDisposition = 0xC0000026,
        NoncontinuableException = 0xC0000025,
        PrivilegedInstruction = 0xC0000096,
        SingleStep = 0x80000004,
        StackOverflow = 0xC00000FD,

        ClrException = 0xE0434352,

        ClrDbgNotificationExceptionCode = 0x04242420
    }
}

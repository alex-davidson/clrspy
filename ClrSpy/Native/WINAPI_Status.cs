namespace ClrSpy.Native
{
    public static class WINAPI_Status
    {
        public const int S_OK = 0;
        public const int S_FALSE = 1;
        public const uint E_PENDING = 0x8000000A;
        public const uint E_UNEXPECTED = 0x8001FFFF;
        public const uint E_FAIL = 0x80004005;
    }
}

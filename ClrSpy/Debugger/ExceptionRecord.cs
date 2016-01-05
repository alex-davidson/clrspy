using ClrSpy.Native;

namespace ClrSpy.Debugger
{
    public class ExceptionRecord
    {
        public ulong Address { get; set; }
        public ExceptionCode Code { get; set; }
        public ExceptionRecord InnerException { get; set; }
        public ulong[] Parameters { get; set; }
    }
}

namespace ClrSpy.Debugger
{
    public class DebugMemoryDump : IDebugSessionTarget
    {
        public string DumpFilePath { get; }
        public bool Exclusive => true;

        public DebugMemoryDump(string dumpFilePath)
        {
            DumpFilePath = dumpFilePath;
        }

        public DebugSession CreateSession()
        {
            return DebugSession.Load(DumpFilePath);
        }
    }
}

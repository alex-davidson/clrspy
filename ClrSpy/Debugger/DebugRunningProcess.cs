using ClrSpy.Processes;

namespace ClrSpy.Debugger
{
    public class DebugRunningProcess : IDebugSessionTarget
    {
        public IProcessInfo Process { get; }
        public DebugMode Mode { get; }
        public bool Exclusive => Mode == DebugMode.Snapshot || Mode == DebugMode.Control;

        public DebugRunningProcess(IProcessInfo process, DebugMode mode = DebugMode.Observe)
        {
            Process = process;
            Mode = mode;
        }

        public DebugSession CreateSession()
        {
            return DebugSession.Create(Process, Mode);
        }
    }
}

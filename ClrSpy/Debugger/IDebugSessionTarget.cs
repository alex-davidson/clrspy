namespace ClrSpy.Debugger
{
    public interface IDebugSessionTarget
    {
        DebugSession CreateSession();
        bool Exclusive { get; }
    }
}

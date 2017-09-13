using ClrSpy.Architecture;

namespace ClrSpy.Processes
{
    public interface IProcessInfo
    {
        long WorkingSetSizeBytes { get; }
        long VirtualMemorySizeBytes { get; }
        string Name { get; }
        int Pid { get; }
        ProcessArchitecture Architecture { get; }
    }
}
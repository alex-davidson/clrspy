namespace ClrSpy.Processes
{
    public interface IProcessFinder
    {
        IProcessInfo[] FindProcessesByName(string name);
        IProcessInfo VerifyProcessName(string name, int pid);
        IProcessInfo GetProcessById(int pid);
    }
}
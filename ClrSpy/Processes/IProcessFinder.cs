namespace ClrSpy.Processes
{
    public interface IProcessFinder
    {
        IProcessInfo[] FindProcessesByName(string name);
        IProcessInfo[] FindProcessesByAppPoolNamePrefix(string appPoolNamePrefix);
        IProcessInfo VerifyProcessName(string name, int pid);
        IProcessInfo VerifyAppPoolNamePrefix(string appPoolNamePrefix, int pid);
        IProcessInfo GetProcessById(int pid);
    }
}
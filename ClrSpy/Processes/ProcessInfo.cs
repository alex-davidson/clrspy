using System.Diagnostics;
using ClrSpy.Architecture;

namespace ClrSpy.Processes
{
    public class ProcessInfo : IProcessInfo
    {
        public static IProcessInfo FromProcess(Process process, string appPoolName = null)
        {
            return new ProcessInfo {
                Pid = process.Id,
                Name = process.ProcessName,
                VirtualMemorySizeBytes = process.VirtualMemorySize64,
                WorkingSetSizeBytes = process.WorkingSet64,
                Architecture = ProcessArchitecture.FromProcess(process),
                IISAppPoolName = appPoolName,
            };
        }

        public long WorkingSetSizeBytes { get; set; }
        public long VirtualMemorySizeBytes { get; set; }
        public string Name { get; set; }
        public int Pid { get; set; }
        public ProcessArchitecture Architecture { get; set; }
        public string IISAppPoolName { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Processes
{
    public class ProcessInfo : IProcessInfo
    {
        public static IProcessInfo FromProcess(Process process)
        {
            return new ProcessInfo {
                Pid = process.Id,
                Name = process.ProcessName,
                VirtualMemorySizeBytes = process.VirtualMemorySize64,
                WorkingSetSizeBytes = process.WorkingSet64,
                Handle = process.Handle
            };
        }

        public long WorkingSetSizeBytes { get; set; }
        public long VirtualMemorySizeBytes { get; set; }
        public string Name { get; set; }
        public int Pid { get; set; }
        public IntPtr Handle { get;set; }

    }
}
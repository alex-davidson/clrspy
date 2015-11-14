using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Processes
{
    public interface IProcessInfo
    {
        long WorkingSetSizeBytes { get; }
        long VirtualMemorySizeBytes { get; }
        string Name { get; }
        int Pid { get; }
        IntPtr Handle { get; }
    }
}
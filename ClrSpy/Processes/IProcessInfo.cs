using System;
using System.Collections.Generic;
using ClrSpy.Architecture;
using Microsoft.Diagnostics.Runtime;

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
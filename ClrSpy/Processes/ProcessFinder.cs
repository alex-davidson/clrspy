using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Processes
{
    public class ProcessFinder : IProcessFinder
    {
        public IProcessInfo[] FindProcessesByName(string name)
        {
            return Process.GetProcessesByName(name).Select(ProcessInfo.FromProcess).ToArray();
        }

        public IProcessInfo VerifyProcessName(string name, int pid)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var process = GetProcessById(pid);
            if (!name.Equals(process.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new ProcessNotFoundException($"Process with an Id of {pid} is called {process.Name}");
            }
            return process;
        }

        public IProcessInfo GetProcessById(int pid)
        {
            try
            {
                return ProcessInfo.FromProcess(Process.GetProcessById(pid));
            }
            catch (ArgumentException ex)
            {
                throw new ProcessNotFoundException(ex.Message);
            }
        }
        
        public IEnumerable<VersionInfo> EnumerateClrVersions(IProcessInfo info)
        {
            using (var session = DebugSession.Create(info.Pid))
            {
                return session.DataTarget.ClrVersions.Select(v => v.Version);
            }
        }
    }
}

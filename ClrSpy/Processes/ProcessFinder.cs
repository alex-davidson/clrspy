using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ClrSpy.Debugger;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Web.Administration;

namespace ClrSpy.Processes
{
    public class ProcessFinder : IProcessFinder
    {
        public IProcessInfo[] FindProcessesByName(string name)
        {
            return Process.GetProcessesByName(name)
                .Select(p => ProcessInfo.FromProcess(p))
                .ToArray();
        }

        public IProcessInfo[] FindProcessesByAppPoolNamePrefix(string appPoolNamePrefix)
        {
            try
            {
                using (var server = new ServerManager())
                {
                    return server.ApplicationPools
                        .Where(a => a.Name.StartsWith(appPoolNamePrefix, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(a => a.WorkerProcesses
                            .Select(w => Process.GetProcessById(w.ProcessId))
                            .Select(p => ProcessInfo.FromProcess(p, a.Name)))
                        .ToArray();
                }
            }
            catch (COMException ex)
            {
                throw new FeatureUnavailableException("Unable to inspect IIS application pools. Please check that the IIS Management Console is installed on this system.", ex);
            }
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

        public IProcessInfo VerifyAppPoolNamePrefix(string appPoolNamePrefix, int pid)
        {
            if (appPoolNamePrefix == null) throw new ArgumentNullException(nameof(appPoolNamePrefix));
            var process = GetProcessById(pid);
            try
            {
                using (var server = new ServerManager())
                {
                    var pids = server.ApplicationPools
                        .Where(a => a.Name.StartsWith(appPoolNamePrefix, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(a => a.WorkerProcesses)
                        .Select(w => w.ProcessId);
                    if (!pids.Contains(pid))
                    {
                        throw new ProcessNotFoundException($"Process with an Id of {pid} does not appear to be a worker process for a matching application pool");
                    }
                }
            }
            catch (COMException ex)
            {
                throw new FeatureUnavailableException("Unable to inspect IIS application pools. Please check that the IIS Management Console is installed on this system.", ex);
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
        
        public static IEnumerable<VersionInfo> EnumerateClrVersions(IProcessInfo info)
        {
            using (var session = DebugSession.Create(info))
            {
                return session.DataTarget.ClrVersions.Select(v => v.Version);
            }
        }
    }
}

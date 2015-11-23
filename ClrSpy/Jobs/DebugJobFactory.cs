using System;
using System.Collections.Generic;
using System.Linq;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.Processes;

namespace ClrSpy.Jobs
{
    public class DebugJobFactory
    {
        private readonly IProcessFinder processFinder;

        public DebugJobFactory(IProcessFinder processFinder)
        {
            this.processFinder = processFinder;
        }

        public IDebugJob Create(Arguments arguments, ConsoleLog console)
        {
            var process = ResolveTargetProcess(arguments, console);
            switch(arguments.JobType)
            {
                case JobType.DumpStacks:
                    {
                        return new DumpStacksJob(process, arguments.PauseTargetProcess);
                    }

                case JobType.DumpHeap:
                    {
                        if(!arguments.PauseTargetProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to dump heap information.") { ShowUsage = true };
                        return new DumpHeapJob(process, arguments.PauseTargetProcess);
                    }
                    
                default:
                    throw new ErrorWithExitCodeException(1, $"Unsupported operation: {arguments.JobType}");
            }
        }
        
        private IProcessInfo ResolveTargetProcess(Arguments arguments, ConsoleLog console)
        {
            if(arguments.Pid == null)
            {
                if(String.IsNullOrWhiteSpace(arguments.ProcessName)) throw new ErrorWithExitCodeException(1, "No process specified.") { ShowUsage = true };

                var candidates = processFinder.FindProcessesByName(arguments.ProcessName);
                if(!candidates.Any()) throw new ProcessNotFoundException($"No process with name '{arguments.ProcessName}' is running");
                if(candidates.Count() == 1) return candidates.Single();

                DescribeCandidateProcesses(candidates, console);
                throw new ProcessNotFoundException("Please specify a unique process Id using the -p switch.");
            }
            
            if(!String.IsNullOrWhiteSpace(arguments.ProcessName))
            {
                return processFinder.VerifyProcessName(arguments.ProcessName, arguments.Pid.Value);
            }
            return processFinder.GetProcessById(arguments.Pid.Value);
        }

        
        private void DescribeCandidateProcesses(IProcessInfo[] candidates, ConsoleLog console)
        {
            var byArchitecture = candidates.ToLookup(c => c.Architecture);
            var self = ProcessArchitecture.FromCurrentProcess();

            if(self is ProcessArchitecture.x86)
            {
                if(!x86Thunk.Bootstrap.WasUsed)
                {   
                    // Native x86. Write out everything.
                    WriteProcessList(candidates, console);
                }
                else
                {
                    // Write out only matching processes.
                    WriteProcessList(byArchitecture[self].ToList(), console);
                }
            }
            else
            {
                WriteProcessList(byArchitecture[self].ToList(), console);
                if(byArchitecture[new ProcessArchitecture.x86()].Any())
                {
                    // Go to 32-bit and render the rest of the process information.
                    throw new Requires32BitEnvironmentException();
                }
            }
        }

        private void WriteProcessList(IList<IProcessInfo> processes, ConsoleLog console)
        {
            if(!processes.Any()) return;
            var tabulator = new Tabulator(
                    new Column("Pid") { Width = 5, RightAlign = true},
                    new Column("Name") { Width = 20},
                    new Column("Memory") { Width = 12, RightAlign = true},
                    new Column("CLR Versions"))
                    { Defaults = { Padding = 2 } };
            
            console.WriteLine($"{processes.Count} matching {ProcessArchitecture.FromCurrentProcess().Describe()} processes were found:");
            console.WriteLine(tabulator.GetHeader());
            foreach (var candidate in processes.OrderBy(c => c.Pid))
            {
                console.WriteLine(tabulator.Tabulate(candidate.Pid, candidate.Name, $"{candidate.WorkingSetSizeBytes.InKilobytes()} KB", DescribeCLRVersions(candidate)));
            }
        }

        private string DescribeCLRVersions(IProcessInfo process)
        {
            try
            {
                var versions = processFinder.EnumerateClrVersions(process).Select(v => $"{v.Major}.{v.Minor}").Distinct().ToArray();
                if(!versions.Any()) return "-";
                return String.Join(", ", versions);
            }
            catch(Requires32BitEnvironmentException)
            {
                return "(unknown, 32-bit)";
            }
            catch(Requires64BitEnvironmentException)
            {
                return "(unknown, 64-bit)";
            }
        }

    }
}

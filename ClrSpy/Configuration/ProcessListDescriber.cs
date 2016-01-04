using System;
using System.Collections.Generic;
using System.Linq;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class ProcessListDescriber
    {
        public void DescribeCandidateProcesses(IList<IProcessInfo> candidates, ConsoleLog console)
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
                var versions = ProcessFinder.EnumerateClrVersions(process).Select(v => $"{v.Major}.{v.Minor}").Distinct().ToArray();
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
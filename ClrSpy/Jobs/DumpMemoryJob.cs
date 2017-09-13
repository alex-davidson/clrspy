using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Jobs
{
    public class DumpMemoryJob : IDebugJob
    {
        private readonly DebugRunningProcess target;
        public int Pid => target.Process.Pid;

        public string DumpFilePath { get; }
        public bool OverwriteDumpFileIfExists { get; set; }

        public DumpMemoryJob(DebugRunningProcess target, string dumpFilePath)
        {
            DumpFilePath = dumpFilePath;
            this.target = target;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = target.CreateSession())
            {
                if (File.Exists(DumpFilePath))
                {
                    if (!OverwriteDumpFileIfExists) throw new IOException($"File already exists: {DumpFilePath}");
                    File.Delete(DumpFilePath);
                }

                if (session.DataTarget.DebuggerInterface is IDebugClient2 clientInterface)
                {
                    // Record complete dump, including binaries and symbols if possible.
                    clientInterface.WriteDumpFile2(DumpFilePath, DEBUG_DUMP.SMALL, DEBUG_FORMAT.USER_SMALL_FULL_MEMORY | DEBUG_FORMAT.CAB_SECONDARY_ALL_IMAGES, "");
                    return;
                }

                console.WriteLine("WARNING: API only supports old-style dump? Recording minidump instead.");
                session.DataTarget.DebuggerInterface.WriteDumpFile(DumpFilePath, DEBUG_DUMP.SMALL);
            }
        }
    }
}

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
                var clientInterface = session.DataTarget.DebuggerInterface as IDebugClient2;
                if (clientInterface == null)
                {
                    console.WriteLine("WARNING: API only supports old-style dump? Recording minidump instead.");
                    session.DataTarget.DebuggerInterface.WriteDumpFile(DumpFilePath, DEBUG_DUMP.SMALL);
                    return;
                }
                clientInterface.WriteDumpFile2(DumpFilePath, DEBUG_DUMP.SMALL, DEBUG_FORMAT.USER_SMALL_FULL_MEMORY | DEBUG_FORMAT.CAB_SECONDARY_ALL_IMAGES, "");
            }
        }
    }
}

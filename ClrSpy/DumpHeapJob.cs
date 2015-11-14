using System;
using System.IO;
using ClrSpy.CliSupport;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy
{
    public class DumpHeapJob
    {
        private readonly int pid;
        
        public DumpHeapJob(int pid, bool exclusive)
        {
            if(!exclusive) throw new ArgumentException("Heap analysis requires suspending the target process.");
            this.pid = pid;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = DebugSession.Create(pid, true))
            {
                var runtime = session.CreateRuntime();
                WriteHeapInfo(runtime, output);
            }
        }
        
        private void WriteHeapInfo(ClrRuntime runtime, TextWriter output)
        {
        }
    }
}
using System;
using System.IO;
using ClrSpy.CliSupport;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Jobs
{
    public class DumpHeapJob : IDebugJob
    {
        public int Pid { get; }

        public DumpHeapJob(int pid, bool exclusive)
        {
            if(!exclusive) throw new ArgumentException("Heap analysis requires suspending the target process.");
            this.Pid = pid;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = DebugSession.Create(Pid, true))
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
using System;
using System.Diagnostics;
using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Native;
using Microsoft.Diagnostics.Runtime;
using System.Linq;

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
            GetArchitectureDependency().Assert();
            using (var target = AcquireTarget(pid))
            {
                var runtime = CreateRuntime(target);
                WriteHeapInfo(runtime, output);
            }
        }
        
        private ArchitectureDependency GetArchitectureDependency()
        {
            var p = Process.GetProcessById(pid);
            if(NativeWrappers.IsWin64(p)) return new ArchitectureDependency.x64();
            return new ArchitectureDependency.x86();
        }
        
        private void WriteHeapInfo(ClrRuntime runtime, TextWriter output)
        {
        }
        
        private static void Swap(ref ulong start, ref ulong stop)
        {
            var tmp = start;
            start = stop;
            stop = tmp;
        }

        private DataTarget AcquireTarget(int pid)
        {
            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = DataTarget.AttachToProcess(pid, 0, true ? AttachFlag.NonInvasive : AttachFlag.Passive);

            bool isTarget64Bit = dataTarget.PointerSize == 8;
            AssertCorrectBitness(isTarget64Bit);

            return dataTarget;
        }

        private static ClrRuntime CreateRuntime(DataTarget target)
        {
            if(!target.ClrVersions.Any()) throw new ErrorWithExitCodeException(2, "Target process does not appear to contain any CLR modules.");

            // Assume there's at most one CLR in the process:
            var version = target.ClrVersions[0];
            return version.CreateRuntime();
        }

        private static void AssertCorrectBitness(bool isTarget64Bit)
        {
            // Should never fail this check:
            if (Environment.Is64BitProcess != isTarget64Bit)
                throw new ErrorWithExitCodeException(255, $"Architecture mismatch:  Process is {(Environment.Is64BitProcess ? "64 bit" : "32 bit")} but target is {(isTarget64Bit ? "64 bit" : "32 bit")}");
        }

    }
}
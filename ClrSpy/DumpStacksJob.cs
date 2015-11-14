using System;
using System.Diagnostics;
using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Native;
using Microsoft.Diagnostics.Runtime;
using System.Linq;

namespace ClrSpy
{
    public class DumpStacksJob
    {
        private readonly int pid;
        private readonly bool exclusive;

        public bool DumpStackObjects { get; set; }

        public DumpStacksJob(int pid, bool exclusive)
        {
            this.pid = pid;
            this.exclusive = exclusive;
            DumpStackObjects = exclusive;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            GetArchitectureDependency().Assert();
            using (var target = AcquireTarget(pid))
            {
                var runtime = CreateRuntime(target);
                WriteThreadInfo(runtime, output);
            }
        }
        
        private ArchitectureDependency GetArchitectureDependency()
        {
            var p = Process.GetProcessById(pid);
            if(NativeWrappers.IsWin64(p)) return new ArchitectureDependency.x64();
            return new ArchitectureDependency.x86();
        }
        
        private void WriteThreadInfo(ClrRuntime runtime, TextWriter output)
        {
            // Walk each thread in the process.
            foreach (var thread in runtime.Threads)
            {
                if (!thread.IsAlive) continue;

                output.WriteLine("Thread {0:X}:", thread.OSThreadId);
                output.WriteLine("Stack: {0:X} - {1:X}", thread.StackBase, thread.StackLimit);

                var exception = thread.CurrentException;
                if (exception != null)
                {
                    output.WriteLine("Exception: {0:X} ({1}), HRESULT={2:X}", exception.Address, exception.Type.Name, exception.HResult);
                }
                
                output.WriteLine(); 

                WriteStackTrace(thread, output);

                // Print a !DumpStackObjects equivalent.
                if (DumpStackObjects)
                {
                    output.WriteLine();
                    WriteStackObjects(runtime, thread, output);
                }

                output.WriteLine();
                output.WriteLine("----------------------------------");
                output.WriteLine();
            }
        }

        private static void WriteStackObjects(ClrRuntime runtime, ClrThread thread, TextWriter output)
        {
            var heap = runtime.GetHeap();
            
            ulong start = thread.StackBase;
            ulong stop = thread.StackLimit;

            if (start > stop) Swap(ref start, ref stop);

            output.WriteLine("Stack objects:");

            for (ulong ptr = start; ptr <= stop; ptr += (ulong) runtime.PointerSize)
            {
                ulong obj;
                if (!runtime.ReadPointer(ptr, out obj)) break;

                var type = heap.GetObjectType(obj);
                if (type == null) continue;
                if (type.IsFree) continue;

                output.WriteLine("{0,16:X} {1,16:X} {2}", ptr, obj, type.Name);
            }
        }

        private static void Swap(ref ulong start, ref ulong stop)
        {
            var tmp = start;
            start = stop;
            stop = tmp;
        }

        private static void WriteStackTrace(ClrThread thread, TextWriter output)
        {
            output.WriteLine("Managed Callstack:");
            foreach (var frame in thread.StackTrace)
            {
                // Note that CLRStackFrame currently only has three pieces of data: stack pointer,
                // instruction pointer, and frame name (which comes from ToString).  Future
                // versions of this API will allow you to get the type/function/module of the
                // method (instead of just the name).  This is not yet implemented.
                output.WriteLine("{0,16:X} {1,16:X} {2}", frame.StackPointer, frame.InstructionPointer, frame.DisplayString);
            }
        }


        private DataTarget AcquireTarget(int pid)
        {
            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = DataTarget.AttachToProcess(pid, 0, exclusive ? AttachFlag.NonInvasive : AttachFlag.Passive);

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
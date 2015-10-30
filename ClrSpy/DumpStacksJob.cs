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

        public DumpStacksJob(int pid, bool exclusive)
        {
            this.pid = pid;
            this.exclusive = exclusive;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            GetArchitectureDependency().Assert();
            WriteThreadStacks(output);
        }
        
        private ArchitectureDependency GetArchitectureDependency()
        {
            var p = Process.GetProcessById(pid);
            if(NativeWrappers.IsWin64(p)) return new ArchitectureDependency.x64();
            return new ArchitectureDependency.x86();
        }

        
        private void WriteThreadStacks(TextWriter output)
        {
            ClrRuntime runtime = CreateRuntime(pid);

            // Walk each thread in the process.
            foreach (ClrThread thread in runtime.Threads)
            {
                // The ClrRuntime.Threads will also report threads which have recently died, but their
                // underlying datastructures have not yet been cleaned up.  This can potentially be
                // useful in debugging (!threads displays this information with XXX displayed for their
                // OS thread id).  You cannot walk the stack of these threads though, so we skip them
                // here.
                if (!thread.IsAlive)
                    continue;

                output.WriteLine("Thread {0:X}:", thread.OSThreadId);
                output.WriteLine("Stack: {0:X} - {1:X}", thread.StackBase, thread.StackLimit);

                // Each thread tracks a "last thrown exception".  This is the exception object which
                // !threads prints.  If that exception object is present, we will display some basic
                // exception data here.  Note that you can get the stack trace of the exception with
                // ClrHeapException.StackTrace (we don't do that here).
                ClrException exception = thread.CurrentException;
                if (exception != null)
                    output.WriteLine("Exception: {0:X} ({1}), HRESULT={2:X}", exception.Address, exception.Type.Name, exception.HResult);

                // Walk the stack of the thread and print output similar to !ClrStack.
                output.WriteLine();
                output.WriteLine("Managed Callstack:");
                foreach (ClrStackFrame frame in thread.StackTrace)
                {
                    // Note that CLRStackFrame currently only has three pieces of data: stack pointer,
                    // instruction pointer, and frame name (which comes from ToString).  Future
                    // versions of this API will allow you to get the type/function/module of the
                    // method (instead of just the name).  This is not yet implemented.
                    output.WriteLine("{0,16:X} {1,16:X} {2}", frame.StackPointer, frame.InstructionPointer, frame.DisplayString);
                }

                var dso = false;
                // Print a !DumpStackObjects equivalent.
                if (dso)
                {
                    // We'll need heap data to find objects on the stack.
                    ClrHeap heap = runtime.GetHeap();

                    // Walk each pointer aligned address on the stack.  Note that StackBase/StackLimit
                    // is exactly what they are in the TEB.  This means StackBase > StackLimit on AMD64.
                    ulong start = thread.StackBase;
                    ulong stop = thread.StackLimit;

                    // We'll walk these in pointer order.
                    if (start > stop)
                    {
                        ulong tmp = start;
                        start = stop;
                        stop = tmp;
                    }

                    output.WriteLine();
                    output.WriteLine("Stack objects:");

                    // Walk each pointer aligned address.  Ptr is a stack address.
                    for (ulong ptr = start; ptr <= stop; ptr += (ulong)runtime.PointerSize)
                    {
                        // Read the value of this pointer.  If we fail to read the memory, break.  The
                        // stack region should be in the crash dump.
                        ulong obj;
                        if (!runtime.ReadPointer(ptr, out obj))
                            break;

                        // 003DF2A4 
                        // We check to see if this address is a valid object by simply calling
                        // GetObjectType.  If that returns null, it's not an object.
                        ClrType type = heap.GetObjectType(obj);
                        if (type == null)
                            continue;

                        // Don't print out free objects as there tends to be a lot of them on
                        // the stack.
                        if (!type.IsFree)
                            output.WriteLine("{0,16:X} {1,16:X} {2}", ptr, obj, type.Name);
                    }
                }

                output.WriteLine();
                output.WriteLine("----------------------------------");
                output.WriteLine();
            }
        }
        


        private ClrRuntime CreateRuntime(int pid)
        {
            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = DataTarget.AttachToProcess(pid, 0, exclusive ? AttachFlag.NonInvasive : AttachFlag.Passive);
            
            bool isTarget64Bit = dataTarget.PointerSize == 8;
            AssertCorrectBitness(isTarget64Bit);

            if(!dataTarget.ClrVersions.Any()) throw new ErrorWithExitCodeException(2, "Target process does not appear to contain any CLR modules.");

            // Assume there's at most one CLR in the process:
            var version = dataTarget.ClrVersions[0];
            
            return version.CreateRuntime();
        }

        private static void AssertCorrectBitness(bool isTarget64Bit)
        {
            // Should never fail this check:
            if (Environment.Is64BitProcess != isTarget64Bit)
                throw new ErrorWithExitCodeException(255, String.Format("Architecture mismatch:  Process is {0} but target is {1}", Environment.Is64BitProcess ? "64 bit" : "32 bit", isTarget64Bit ? "64 bit" : "32 bit"));
        }

    }
}
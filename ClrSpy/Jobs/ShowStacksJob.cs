using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Jobs
{
    public class ShowStacksJob : IDebugJob
    {
        private readonly IDebugSessionTarget target;
        public int? Pid => (target as DebugRunningProcess)?.Process.Pid;

        public bool ShowStackObjects { get; set; }

        public ShowStacksJob(IDebugSessionTarget target)
        {
            this.target = target;
            ShowStackObjects = target.Exclusive;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = target.CreateSession())
            {
                var runtime = session.CreateRuntime();
                WriteThreadInfo(runtime, output);
            }
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
                if (ShowStackObjects)
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
            var heap = runtime.Heap;
            
            var start = thread.StackBase;
            var stop = thread.StackLimit;

            if (start > stop) Util.Swap(ref start, ref stop);

            output.WriteLine("Stack objects:");

            for (var ptr = start; ptr <= stop; ptr += (ulong) runtime.PointerSize)
            {
                if (!runtime.ReadPointer(ptr, out var obj)) break;

                var type = heap.GetObjectType(obj);
                if (type == null) continue;
                if (type.IsFree) continue;

                output.WriteLine("{0,16:X} {1,16:X} {2}", ptr, obj, type.Name);
            }
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
    }
}
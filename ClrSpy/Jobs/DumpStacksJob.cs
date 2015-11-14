using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Processes;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Jobs
{
    public class DumpStacksJob : IDebugJob
    {
        private readonly IProcessInfo process;
        public int Pid => process.Pid;
        public bool Exclusive { get; }

        public bool DumpStackObjects { get; set; }

        public DumpStacksJob(IProcessInfo process, bool exclusive)
        {
            this.process = process;
            this.Exclusive = exclusive;
            DumpStackObjects = exclusive;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = DebugSession.Create(process, Exclusive))
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

            if (start > stop) Util.Swap(ref start, ref stop);

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
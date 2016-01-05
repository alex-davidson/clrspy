using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
using ClrSpy.Native;
using ClrSpy.Processes;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Jobs
{
    public class TraceExceptionsJob : IDebugJob
    {
        private readonly IProcessInfo process;
        private readonly ICancellationEvents cancellationEvents;
        public int Pid => process.Pid;

        public TraceExceptionsJob(IProcessInfo process, ICancellationEvents cancellationEvents)
        {
            this.process = process;
            this.cancellationEvents = cancellationEvents;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (ForcedTerminationMonitor.HoldOpen())
            {
                cancellationEvents.LogRequestsTo(console);
                using (var session = DebugSession.Create(process, DebugMode.Control))
                {
                    var eventReceiver = new StructuredEventReceiver(session, output);

                    session.DataTarget.SetEventListener(eventReceiver);

                    console.WriteLine("Listening for exceptions. Press CTRL-C to exit.");

                    while (session.DataTarget.RunCallbacks(cancellationEvents.Token))
                    {
                        session.DataTarget.ResumeExecution();
                    }
                }
            }
        }

        class StructuredEventReceiver : DebugEventListenerBase
        {
            private readonly DebugSession session;
            private readonly TextWriter writer;
            private ClrRuntime runtime;

            public StructuredEventReceiver(DebugSession session, TextWriter output)
            {
                this.session = session;
                runtime = session.CreateRuntime();
                writer = TextWriter.Synchronized(output);
            }

            public override DEBUG_STATUS OnException(DataTarget debugger, EXCEPTION_RECORD64 nativeException, bool firstChance)
            {
                var exception = new ExceptionRecordReader().Read(nativeException);
                writer.WriteLine($"{exception.Address}");
                return DEBUG_STATUS.NO_CHANGE;
            }

            public override DEBUG_STATUS OnExitProcess(DataTarget debugger, uint exitCode)
            {
                writer.WriteLine($"Target process terminated with code {exitCode}");
                return DEBUG_STATUS.NO_CHANGE;
            }

            public override DEBUG_EVENT GetInterestMask()
            {
                return DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS;
            }
        }
    }
}

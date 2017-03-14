using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ClrSpy.Debugger;
using ClrSpy.Native;
using ClrSpy.Processes;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using NUnit.Framework;

namespace ClrSpy.UnitTests.TraceExceptions
{
    [TestFixture]
    public class RecordExceptionEventsTests
    {
        [Test, Repeat(10)]
        public void CanRecordExceptionEvents()
        {
            var eventReceiver = new ExceptionEventRecorder();

            using (var thrower = new ExceptionThrowerProcess())
            {
                var process = thrower.Start();
                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Control))
                {
                    session.DataTarget.SetEventListener(eventReceiver);

                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    thrower.TriggerException();

                    while(session.DataTarget.RunCallbacks(cts.Token))
                    {
                        if (cts.Token.IsCancellationRequested) break;

                        var status = session.DataTarget.GetExecutionStatus();

                        var adv = session.DataTarget.DebuggerInterface as IDebugAdvanced;
                        var runtime = session.CreateRuntime();
                        
                        //session.DataTarget.ResumeExecution();
                    }
                }
            }

            Debug.WriteLine(String.Join(", ", eventReceiver.Exceptions.Select(e => PrettyPrintCode(e.Code))));
            Assert.That(eventReceiver.Exceptions, Has.Count.EqualTo(2)); // First-chance and second-chance.
        }

        private static string PrettyPrintCode<T>(T enumValue) where T : struct, IFormattable
        {
            if (Enum.IsDefined(typeof(T), enumValue)) return enumValue.ToString();
            return $"{enumValue:X}";
        }

        class ExceptionEventRecorder : DebugEventListenerBase
        {
            public override DEBUG_EVENT GetInterestMask()
            {
                return DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS;
            }

            public List<ExceptionRecord> Exceptions { get; } = new List<ExceptionRecord>();
            public uint? ExitCode { get; private set; }

            public override DEBUG_STATUS OnException(DataTarget debugger, EXCEPTION_RECORD64 nativeException, bool isFirstChance)
            {
                var exception = new ExceptionRecordReader().Read(nativeException);
                
                if (exception.Code == ExceptionCode.ClrDbgNotificationExceptionCode) return DEBUG_STATUS.NO_CHANGE; // Something to do with process initialisation.
                if (exception.Code == ExceptionCode.Breakpoint) return DEBUG_STATUS.BREAK; // I have no idea where this comes from.
                Exceptions.Add(exception);
                return DEBUG_STATUS.BREAK;
            }

            public override DEBUG_STATUS OnExitProcess(DataTarget debugger, uint exitCode)
            {
                ExitCode = exitCode;
                return DEBUG_STATUS.NO_CHANGE;
            }
        }
    }
}

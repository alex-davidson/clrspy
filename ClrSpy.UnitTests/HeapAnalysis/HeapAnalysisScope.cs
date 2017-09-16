using System;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.Jobs;
using ClrSpy.Processes;
using ClrSpy.UnitTests.Utils;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.UnitTests.HeapAnalysis
{
    public class HeapAnalysisScope : IDisposable
    {
        private readonly IDisposable cleanup;

        public HeapAnalysisScope(IDisposable cleanup, ClrClassObject subject)
        {
            this.cleanup = cleanup;
            Subject = subject;
        }

        public void Dispose()
        {
            cleanup.Dispose();
        }

        public ClrClassObject Subject { get; }

        public ClrType FindClrType<TActualType>()
        {
            return Subject.Type.Heap.GetTypeByName(typeof(TActualType).FullName);
        }

        public static async Task<HeapAnalysisScope> Create()
        {
            using (var tracker = new DisposableTracker())
            {
                var session = await CreateDebugSession(tracker);
                var subject = GetSubjectFromSession(session);

                return tracker.TransferOwnershipTo(t => new HeapAnalysisScope(t, subject));
            }
        }

        public static HeapAnalysisScope LoadMemoryDump(string filePath)
        {
            using (var tracker = new DisposableTracker())
            {
                var session = tracker.Track(DebugSession.Load(filePath));
                var subject = GetSubjectFromSession(session);

                return tracker.TransferOwnershipTo(t => new HeapAnalysisScope(t, subject));
            }
        }

        public static async Task WriteMemoryDump(string filePath)
        {
            using (var tracker = new DisposableTracker())
            {
                var session = await CreateDebugSession(tracker);
                DumpMemoryJob.DumpSession(session, filePath);
            }
        }

        private static async Task<DebugSession> CreateDebugSession(DisposableTracker tracker)
        {
            var taskTarget = tracker.Track(new HeapAnalysisTargetProcess());
            var process = taskTarget.Start();
            await taskTarget.WaitForTask();

            return tracker.Track(DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot));
        }

        private static ClrClassObject GetSubjectFromSession(DebugSession session)
        {
            var runtime = session.CreateRuntime();
            return runtime.Heap
                .EnumerateAllClrObjects()
                .OfType<ClrClassObject>()
                .Single(i => i.Type.CanBeAssignedTo<HeapAnalysisTarget.Program>());
        }
    }
}

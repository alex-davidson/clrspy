using System;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.Processes;
using ClrSpy.UnitTests.Utils;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
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

        public static async Task<HeapAnalysisScope> Create()
        {
            using (var tracker = new DisposableTracker())
            {
                var taskTarget = tracker.Track(new HeapAnalysisTargetProcess());
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                var session = tracker.Track(DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot));
                var runtime = session.CreateRuntime();
                var subject = runtime.Heap
                    .EnumerateAllClrObjects()
                    .OfType<ClrClassObject>()
                    .Single(i => i.Type.CanBeAssignedTo<HeapAnalysisTarget.Program>());

                return tracker.TransferOwnershipTo(t => new HeapAnalysisScope(t, subject));
            }
        }
    }
}

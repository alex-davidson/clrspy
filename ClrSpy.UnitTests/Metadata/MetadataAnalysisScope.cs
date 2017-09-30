using System;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.Jobs;
using ClrSpy.Processes;
using ClrSpy.UnitTests.Utils;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.UnitTests.Metadata
{
    public class MetadataAnalysisScope : IDisposable
    {
        private readonly IDisposable cleanup;
        private readonly DebugSession session;
        public ClrRuntime Runtime { get; }

        public MetadataAnalysisScope(IDisposable cleanup, DebugSession session)
        {
            this.cleanup = cleanup;
            this.session = session;
            Runtime = session.CreateRuntime();
        }

        public void Dispose()
        {
            cleanup.Dispose();
        }

        public ClrType FindClrType<TActualType>()
        {
            return Runtime.Heap.GetTypeByName(typeof(TActualType).FullName);
        }

        
        public static async Task<MetadataAnalysisScope> Create()
        {
            using (var tracker = new DisposableTracker())
            {
                var session = await CreateDebugSession(tracker);

                return tracker.TransferOwnershipTo(t => new MetadataAnalysisScope(t, session));
            }
        }

        public static MetadataAnalysisScope LoadMemoryDump(string filePath)
        {
            using (var tracker = new DisposableTracker())
            {
                var session = tracker.Track(DebugSession.Load(filePath));

                return tracker.TransferOwnershipTo(t => new MetadataAnalysisScope(t, session));
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
            var taskTarget = tracker.Track(new MetadataAnalysisTargetProcess());
            var process = taskTarget.Start();
            await taskTarget.WaitForTask();

            return tracker.Track(DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot));
        }
    }
}

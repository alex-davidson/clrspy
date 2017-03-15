using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.Processes;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class HeapEnumerationTests
    {
        [Test]
        public async Task CanEnumerateAllObjects()
        {
            using (var taskTarget = new HeapAnalysisTargetProcess())
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();
                    var allObjects = runtime.Heap.EnumerateAllClrObjects().Where(o => !(o is ClrPrimitive)).ToArray();
                    Assert.That(allObjects, Is.Not.Empty);
                    Assert.That(allObjects, Is.Unique);
                }
            }
        }

        [Test]
        public async Task CanEnumerateLiveObjects()
        {
            using (var taskTarget = new HeapAnalysisTargetProcess())
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();
                    var liveObjects = runtime.Heap.EnumerateLiveClrObjects().Where(o => !(o is ClrPrimitive)).ToArray();
                    Assert.That(liveObjects, Is.Not.Empty);
                    Assert.That(liveObjects, Is.Unique);
                }
            }
        }

        [Test]
        public async Task LiveObjectsIsSubsetOfAllObjects()
        {
            using (var taskTarget = new HeapAnalysisTargetProcess())
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();
                    var allObjects = runtime.Heap.EnumerateAllClrObjects().ToArray();
                    var liveObjects = runtime.Heap.EnumerateLiveClrObjects().ToArray();

                    // Need to specify default EqualityComparer in order to use the IEquatable interface. Possible NUnit bug.
                    Assert.That(liveObjects,
                        Is.SubsetOf(allObjects)
                            .Using<IClrObject>(EqualityComparer<IClrObject>.Default));
                }
            }
        }
    }
}

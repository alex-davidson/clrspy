using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis.Tasks;
using ClrSpy.Processes;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Tasks
{
    [TestFixture]
    public class ClrHeapTaskAnalyserTests
    {
        [Test, Repeat(5)]
        public async Task CanDetectTasksInTargetProcess()
        {
            using (var taskTarget = new AsyncTaskTargetProcess())
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();

                    var graph = new ClrHeapTaskAnalyser(runtime).BuildTaskGraph();
                    Assert.That(graph.TaskVertices.Count(), Is.GreaterThanOrEqualTo(2));

                    var simpleTestMatcher = new ContinuationTypeMatcher(taskTarget.GetMethodInfo("SimpleTest"));
                    Assert.That(graph.AllVertices.Count(simpleTestMatcher.IsMatch), Is.EqualTo(1));
                }
            }
        }

        [Test, Repeat(5)]
        public async Task CanInterpretWhenAllTaskInTargetProcess()
        {
            using (var taskTarget = new AsyncTaskTargetProcess("WhenAll"))
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();

                    var graph = new ClrHeapTaskAnalyser(runtime).BuildTaskGraph();
                    Assert.That(graph.TaskVertices.Count(), Is.GreaterThanOrEqualTo(2));

                    var whenAllTestMatcher = new ContinuationTypeMatcher(taskTarget.GetMethodInfo("WhenAllTest"));
                    Assert.That(graph.AllVertices.Count(whenAllTestMatcher.IsMatch), Is.EqualTo(1));
                }
            }
        }
    }
}

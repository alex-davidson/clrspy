using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis.Inspectors;
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

        [Test]
        public async Task CanInterpretDelegateContinuationInTargetProcess()
        {
            using (var taskTarget = new AsyncTaskTargetProcess("ContinueWithDelegate"))
            {
                var process = taskTarget.Start();
                await taskTarget.WaitForTask();

                using (var session = DebugSession.Create(ProcessInfo.FromProcess(process), DebugMode.Snapshot))
                {
                    var runtime = session.CreateRuntime();

                    var graph = new ClrHeapTaskAnalyser(runtime).BuildTaskGraph();
                    Assert.That(graph.TaskVertices.Count(), Is.GreaterThanOrEqualTo(2));

                    var continueWithDelegateTestMatcher = new ContinuationActionTypeMatcher<Task<int>>();
                    var vertex = graph.AllVertices.Single(continueWithDelegateTestMatcher.IsMatch);
                    var description = new ContinuationInspector().Inspect(vertex);

                    Assert.That(description.Source.LineRanges, Is.EqualTo(new[]
                    {
                        new LineRange { Begin = 46, End = 46 }
                    })); 
                }
            }
        }

        [Test]
        public async Task CanResolveSourceFileOfWhenAllTaskContinuationInTargetProcess()
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
                    var vertex = graph.AllVertices.Single(whenAllTestMatcher.IsMatch);

                    var description = new ContinuationInspector().Inspect(vertex);
                }
            }
        }

    }
}

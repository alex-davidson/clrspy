using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.HeapAnalysis.Tasks;

namespace ClrSpy.Jobs
{
    public class ShowTasksJob : IDebugJob
    {
        private readonly IDebugSessionTarget target;
        public int? Pid => (target as DebugRunningProcess)?.Process.Pid;

        public ShowTasksJob(IDebugSessionTarget target)
        {
            this.target = target;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            using (var session = target.CreateSession())
            {
                var runtime = session.CreateRuntime();
                var graph = new ClrHeapTaskAnalyser(runtime).BuildTaskGraph();
                new TaskTreeWalker(graph, new IndentedLineWriter(output)).WalkTree();
            }
        }

        class TaskTreeWalker
        {
            private readonly TaskGraph graph;
            private readonly IndentedLineWriter output;
            private readonly ClrHeapTaskContinuationInspector inspector = new ClrHeapTaskContinuationInspector();

            public TaskTreeWalker(TaskGraph graph, IndentedLineWriter output)
            {
                this.graph = graph;
                this.output = output;
            }

            public void WalkTree()
            {
                foreach (var task in graph.GetRoots())
                {
                    WalkTask(task, 0);
                    output.WriteLine();
                }
            }

            private void WalkTask(ClrClassObject task, int waitingOnCount)
            {
                var description = inspector.Inspect(task);
                output.WriteLine(DescribeTask(description, waitingOnCount));
                using (output.Indent())
                {
                    var waiting = graph.Outgoing(task).ToArray();
                    WalkContinuations(waiting);
                }
            }

            private void WalkContinuations(IClrCompositeObject[] waiting)
            {
                foreach (var continuation in waiting)
                {
                    var waitingOnCount = graph.CountIncoming(continuation);
                    if (continuation.Type.CanBeAssignedTo<Task>())
                    {
                        var task = (ClrClassObject)continuation;
                        WalkTask(task, waitingOnCount);
                    }
                    else
                    {
                        var description = inspector.Inspect(continuation);
                        output.WriteLine(DescribeContinuation(description, waitingOnCount));
                    }
                }
            }

            private static string DescribeTask(ContinuationDetails task, int waitingOnCount)
            {
                if (waitingOnCount > 1)
                {
                    return $"{task.Summary} ({waitingOnCount} antecedents)";
                }
                return task.Summary;
            }

            private static string DescribeContinuation(ContinuationDetails continuation, int waitingOnCount)
            {
                if (waitingOnCount > 1)
                {
                    return $"{continuation.Summary} (waiting on {waitingOnCount - 1} others)";
                }
                return continuation.Summary;
            }
        }
    }
}

using System.IO;
using System.Linq;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.HeapAnalysis.Tasks;
using ClrSpy.Processes;

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
                foreach (var task in graph.TaskVertices)
                {
                    var waiting = graph.Outgoing(task).ToArray();
                    output.WriteLine($"{DescribeTask(task)} ({waiting.Length} continuations)");
                    foreach (var continuation in waiting)
                    {
                        output.WriteLine($"    {DescribeContinuation(graph, continuation)}");
                    }
                    output.WriteLine();
                }
            }
            console.WriteLine("Not implemented.");
        }
        
        private string DescribeObject(IClrCompositeObject obj)
        {
            return $"{obj.Type.Name} @ {obj.Address:X16}";
        }

        private string DescribeTask(ClrClassObject task)
        {
            return DescribeObject(task);
        }

        private string DescribeContinuation(TaskGraph graph, IClrCompositeObject continuation)
        {
            var others = graph.CountIncoming(continuation) - 1;
            if (others > 0)
            {
                return $"{DescribeObject(continuation)} (waiting on {others} others)";
            }
            return DescribeObject(continuation);
        }
    }
}

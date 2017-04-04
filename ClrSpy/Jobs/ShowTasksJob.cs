using System;
using System.Collections.Generic;
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
                    if (graph.CountIncoming(task) > 0) continue;
                    WalkTask(new IndentedTextWriter(output), graph, task, 0);
                    output.WriteLine();
                }
            }
        }

        class IndentedTextWriter
        {
            private int depth = 0;
            private readonly TextWriter inner;

            public IndentedTextWriter(TextWriter inner)
            {
                this.inner = inner;
            }

            public IndentScope Indent()
            {
                return new IndentScope(this);
            }

            public void WriteLine(string line)
            {
                inner.Write(new String(' ', depth * 4));
                inner.WriteLine(line);
            }

            public struct IndentScope : IDisposable
            {
                private readonly IndentedTextWriter writer;

                public IndentScope(IndentedTextWriter writer)
                {
                    this.writer = writer;
                    writer.depth++;
                }

                public void Dispose()
                {
                    writer.depth--;
                }
            }
        }

        private void WalkTask(IndentedTextWriter output, TaskGraph graph, ClrClassObject task, int waitedOnCount)
        {
            if (waitedOnCount > 1)
            {
                output.WriteLine($"{DescribeTask(task)} ({waitedOnCount} antecedents)");
            }
            else
            {
                output.WriteLine(DescribeTask(task));
            }
            using (output.Indent())
            {
                var waiting = graph.Outgoing(task).ToArray();
                WalkContinuations(output, graph, waiting);
            }
        }

        private void WalkContinuations(IndentedTextWriter output, TaskGraph graph, IClrCompositeObject[] waiting)
        {
            foreach (var continuation in waiting)
            {
                var maybeTask = continuation as ClrClassObject;
                if (maybeTask != null && maybeTask.IsOfTaskType())
                {
                    var waitedOnCount = graph.CountIncoming(maybeTask);
                    WalkTask(output, graph, maybeTask, waitedOnCount);
                }
                else
                {
                    output.WriteLine(DescribeContinuation(graph, continuation));
                }
            }
        }

        private static string DescribeObject(IClrCompositeObject obj)
        {
            return $"{obj.Type.Name} @ {obj.Address:X16}";
        }

        private static string DescribeTask(ClrClassObject task)
        {
            return DescribeObject(task);
        }

        private static string DescribeContinuation(TaskGraph graph, IClrCompositeObject continuation)
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

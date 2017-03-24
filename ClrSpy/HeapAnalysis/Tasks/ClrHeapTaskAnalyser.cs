using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public class ClrHeapTaskAnalyser
    {
        private readonly ClrHeap heap;

        public ClrHeapTaskAnalyser(ClrRuntime runtime)
        {
            heap = runtime.GetHeap();
        }

        public TaskGraph BuildTaskGraph()
        {
            var tasks = new TaskGraph();
            var explorer = new ClrHeapTaskContinuationExplorer();
            foreach (var taskObj in heap.EnumerateLiveClrObjects().OfTaskType())
            {
                var collector = new ContinuationCollector(tasks, taskObj);
                explorer.CollectContinuationsFromTask(taskObj, collector);
            }
            return tasks;
        }

        class ContinuationCollector : IContinuationCollector
        {
            private readonly TaskGraph graph;
            private readonly ClrClassObject currentTask;

            public ContinuationCollector(TaskGraph graph, ClrClassObject currentTask)
            {
                this.graph = graph;
                this.currentTask = currentTask;
            }

            public void AddAsyncStateMachineContinuation(IClrCompositeObject continueWith) => graph.AddContinuation(currentTask, continueWith);
            public void AddDelegateContinuation(IClrCompositeObject continueWith) => graph.AddContinuation(currentTask, continueWith);
            public void AddUnknownContinuation(IClrCompositeObject continueWith) => graph.AddContinuation(currentTask, continueWith);
            public void AddITaskCompletionActionContinuation(IClrCompositeObject continueWith) => graph.AddContinuation(currentTask, continueWith);
            public void AddTaskContinuation(ClrClassObject continueWith) => graph.AddContinuation(currentTask, continueWith);
        }
    }
}

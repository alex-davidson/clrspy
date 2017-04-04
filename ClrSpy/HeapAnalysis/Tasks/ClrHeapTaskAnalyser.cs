using System;
using System.Diagnostics;
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
            private static readonly ClrTaskContinuationClassifier classifier = new ClrTaskContinuationClassifier();

            public ContinuationCollector(TaskGraph graph, ClrClassObject currentTask)
            {
                this.graph = graph;
                this.currentTask = currentTask;
            }

            public void AddAsyncStateMachineContinuation(IClrCompositeObject continueWith) => AddWithClassification(continueWith, classifier.IsIAsyncStateMachine);
            public void AddDelegateContinuation(IClrCompositeObject continueWith) => AddWithClassification(continueWith, classifier.IsDelegate);
            public void AddUnknownContinuation(IClrCompositeObject continueWith) => AddWithClassification(continueWith, classifier.IsUnknown);
            public void AddITaskCompletionActionContinuation(IClrCompositeObject continueWith) => AddWithClassification(continueWith, classifier.IsITaskCompletionAction);
            public void AddTaskContinuation(ClrClassObject continueWith) => AddWithClassification(continueWith, classifier.IsTask);

            private void AddWithClassification<T>(T continueWith, Func<T, bool> matchesClassification) where T : IClrCompositeObject
            {
                Debug.Assert(matchesClassification(continueWith), $"Object should match classification {matchesClassification.Method.Name} but did not: {continueWith.Type.Name}");
                graph.AddContinuation(currentTask, continueWith);
            }
        }
    }
}

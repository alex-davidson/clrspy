using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public class ClrHeapTaskContinuationExplorer
    {
        public void CollectContinuationsFromTask(ClrClassObject task, IContinuationCollector collector)
        {
            var continuation = task.GetFieldValue<IClrObject>("m_continuationObject");
            if (continuation == null) return;
            if (IsCompletionSentinelObject(continuation)) return;
            if (continuation is IClrCompositeObject)
            {
                CollectContinuationsFromContinuationObject((IClrCompositeObject)continuation, collector);
                return;
            }
            Debug.Fail($"Unexpected continuation object: {continuation}");
        }

        private static bool IsCompletionSentinelObject(IClrObject continuation)
        {
            return continuation.Type.Is<object>();
        }

        private void CollectContinuationsFromContinuationObject(IClrCompositeObject continuation, IContinuationCollector collector)
        {
            // Task#GetDelegatesFromContinuationObject

            if (continuation == null) return;
            if (continuation.Type.CanBeAssignedTo<Action>())
            {
                CollectContinuationsFromDelegate(continuation, collector);
            }
            else if (continuation.Type.CanBeAssignedTo("System.Threading.Tasks.TaskContinuation"))
            {
                // TaskContinuation#GetDelegateContinuationsForDebugger: abstract
                // TODO
                collector.AddUnknownContinuation(continuation);
            }
            else if (continuation.Type.CanBeAssignedTo<Task>())
            {
                collector.AddTaskContinuation((ClrClassObject)continuation);
            }
            else if (continuation.Type.CanBeAssignedTo("System.Threading.Tasks.ITaskCompletionAction"))
            {
                // Task#GetDelegatesFromContinuationObject
                //   - Returns the object's Invoke method as an action. Type name is probably more useful.
                collector.AddITaskCompletionActionContinuation(continuation);
            }
            else if (continuation.Type.CanBeAssignedTo<List<object>>())
            {
                CollectContinuationsFromListOfObject(continuation, collector);
            }
            else
            {
                collector.AddUnknownContinuation(continuation);
            }
        }

        private void CollectContinuationsFromDelegate(IClrCompositeObject continuationDelegate, IContinuationCollector collector)
        {
            // AsyncMethodBuilderCore#TryGetStateMachineForDebugger

            var target = continuationDelegate.GetFieldValue<IClrCompositeObject>("_target");
            while (target.Type.CanBeAssignedTo("System.Runtime.CompilerServices.AsyncMethodBuilderCore+ContinuationWrapper"))
            {
                // Something to do with ETW wrapping an async continuation?
                continuationDelegate = target.GetFieldValue<IClrCompositeObject>("m_continuation");
                target = continuationDelegate.GetFieldValue<IClrCompositeObject>("_target");
            }

            if (target.Type.CanBeAssignedTo("System.Runtime.CompilerServices.AsyncMethodBuilderCore+MoveNextRunner"))
            {
                var stateMachine = target.GetFieldValue<IClrCompositeObject>("m_stateMachine");
                collector.AddAsyncStateMachineContinuation(stateMachine);
            }
            else
            {
                collector.AddDelegateContinuation(continuationDelegate);
            }
        }

        private void CollectContinuationsFromListOfObject(IClrCompositeObject continuation, IContinuationCollector collector)
        {
            var items = continuation.GetFieldValue<ClrArrayObject>("_items");

            for (var i = 0; i < items.Length; i++)
            {
                var element = items.GetElement<IClrCompositeObject>(i);
                CollectContinuationsFromContinuationObject(element, collector);
            }
        }
    }
}

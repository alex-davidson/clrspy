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
                CollectContinuationsFromTaskContinuationDerivedType(continuation, collector);
            }
            else if (continuation.Type.CanBeAssignedTo<Task>())
            {
                collector.AddTaskContinuation((ClrClassObject)continuation);
            }
            else if (continuation.Type.CanBeAssignedTo("System.Threading.Tasks.ITaskCompletionAction"))
            {
                // Task#GetDelegatesFromContinuationObject
                //   - Returns the object's Invoke method as an action. Type name is probably more useful, so
                //     we collect the ITaskCompletionAction instance instead.
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

        private void CollectContinuationsFromTaskContinuationDerivedType(IClrCompositeObject continuation, IContinuationCollector collector)
        {
            // TaskContinuation#GetDelegateContinuationsForDebugger: abstract

            if (continuation.Type.CanBeAssignedTo("System.Threading.Tasks.StandardTaskContinuation"))
            {
                // StandardTaskContinuation#GetDelegateContinuationsForDebugger
                //   - Returns either the Task's m_action if present, or asks the Task for its continuations.
                //     I suspect the former case means that it will start the Task running, while the latter
                //     means that it resolves the Task.
                var task = continuation.GetFieldValue<IClrCompositeObject>("m_task");
                var taskAction = task.GetFieldValue<IClrCompositeObject>("m_action");

                if (taskAction != null)
                {
                    CollectContinuationsFromDelegate(taskAction, collector);
                }
                else
                {
                    CollectContinuationsFromContinuationObject(task, collector);
                }
            }
            else if(continuation.Type.CanBeAssignedTo("System.Threading.Tasks.AwaitTaskContinuation"))
            {
                // System.Threading.Tasks.AwaitTaskContinuation#GetDelegateContinuationsForDebugger

                var action = continuation.GetFieldValue<IClrCompositeObject>("m_action");
                CollectContinuationsFromDelegate(action, collector);
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

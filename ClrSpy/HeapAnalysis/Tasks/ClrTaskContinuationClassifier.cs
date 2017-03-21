using System;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public class ClrTaskContinuationClassifier
    {
        public bool IsIAsyncStateMachine(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo("System.Runtime.CompilerServices.IAsyncStateMachine");
        public bool IsDelegate(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo<Delegate>();
        public bool IsITaskCompletionAction(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo("System.Threading.Tasks.ITaskCompletionAction");
        public bool IsTask(ClrClassObject obj) => obj.Type.CanBeAssignedTo<Task>();

        public bool IsUnknown(IClrCompositeObject obj)
        {
            if (IsIAsyncStateMachine(obj)) return false;
            if (IsDelegate(obj)) return false;
            if (IsITaskCompletionAction(obj)) return false;
            var classObj = obj as ClrClassObject;
            if (IsTask(classObj)) return false;
            return true;
        }
    }
}

using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public interface IContinuationCollector
    {
        void AddAsyncStateMachineContinuation(IClrCompositeObject continueWith);
        void AddDelegateContinuation(IClrCompositeObject continueWith);
        void AddITaskCompletionActionContinuation(IClrCompositeObject continueWith);
        void AddTaskContinuation(ClrClassObject continueWith);
        void AddUnknownContinuation(IClrCompositeObject continueWith);
    }
}

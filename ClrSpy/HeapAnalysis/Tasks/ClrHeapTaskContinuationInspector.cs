using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public class ClrHeapTaskContinuationInspector
    {
        public ContinuationDetails Inspect(IClrCompositeObject obj)
        {
            return new ContinuationDetails { Summary = $"{obj.Type.Name} @ {obj.Address:X16}" };
        }
    }
}

using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public interface IClrCompositeObject : IClrObject, IAddressable
    {
        ClrHeap OwningHeap { get; }

        IClrObject GetFieldValue(string fieldName);
    }
}

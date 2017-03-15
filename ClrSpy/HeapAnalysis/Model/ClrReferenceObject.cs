using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public abstract class ClrReferenceObject
    {
        protected ClrReferenceObject(ClrType type, ulong address)
        {
            Type = type;
            Address = address;
        }

        public ClrHeap OwningHeap => Type.Heap;
        public ClrType Type { get; }
        public ulong Address { get; }

        public override string ToString() => Type.Name;
    }
}

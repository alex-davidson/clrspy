using System.Collections.Generic;
using ClrSpy.HeapAnalysis.Collections;
using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public static class HeapAnalysisExtensions
    {
        public static IClrObject GetClrObject(this ClrHeap heap, ulong address)
        {
            return new ClrObjectReader().ReadFromAddress(heap, address);
        }

        public static IEnumerable<IClrObject> EnumerateAllClrObjects(this ClrHeap heap)
        {
            var reader = new ClrObjectReader();
            foreach (var address in heap.EnumerateObjectAddresses())
            {
                var type = heap.GetObjectType(address);
                if (!reader.IsValidObjectType(type)) continue;
                yield return reader.ReadFromAddress(type, address);
            }
        }

        public static IEnumerable<IClrObject> EnumerateLiveClrObjects(this ClrHeap heap)
        {
            return new LiveObjectWalker(heap).ExploreFromRoots();
        }

        class LiveObjectWalker
        {
            private readonly ClrHeap heap;

            public LiveObjectWalker(ClrHeap heap)
            {
                this.heap = heap;
                visited = new ObjectAddressSet(heap);
            }

            private readonly ObjectAddressSet visited;
            private readonly Stack<ulong> pending = new Stack<ulong>();

            public IEnumerable<IClrObject> ExploreFromRoots()
            {
                var reader = new ClrObjectReader();

                foreach (var root in heap.EnumerateRoots())
                {
                    if (root.Object == 0) continue;
                    if (!visited.Add(root.Object)) continue;
                    if (root.Kind == GCRootKind.Finalizer) continue;
                    if (root.Address == 0) continue;
                    if (!reader.IsValidObjectType(root.Type)) continue;

                    yield return reader.ReadGCRoot(root);
                    CollectChildren(root.Type, root.Object);
                }

                while (pending.Count > 0)
                {
                    var address = pending.Pop();
                    if (!visited.Add(address)) continue;
                    var type = heap.GetObjectType(address);
                    if (!reader.IsValidObjectType(type)) continue;

                    yield return reader.ReadFromAddress(type, address);
                    CollectChildren(type, address);
                }
            }

            private void CollectChildren(ClrType type, ulong address)
            {
                type.EnumerateRefsOfObjectCarefully(address, CollectChild);
            }

            private void CollectChild(ulong a, int _)
            {
                if (a == 0) return;
                if (visited.Contains(a)) return;
                pending.Push(a);
            }
        }
    }
}

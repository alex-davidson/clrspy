using System;
using System.Collections.Generic;
using System.Linq;
using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public class TaskGraph
    {
        private readonly HashSet<ClrClassObject> taskVertices = new HashSet<ClrClassObject>(new FastEqualityComparer());
        private readonly HashSet<IClrCompositeObject> otherVertices = new HashSet<IClrCompositeObject>(new FastEqualityComparer());

        private readonly Dictionary<IClrCompositeObject, List<ContinuationEdge>> incomingEdges = new Dictionary<IClrCompositeObject, List<ContinuationEdge>>(new FastEqualityComparer());
        private readonly Dictionary<ClrClassObject, List<ContinuationEdge>> outgoingEdges = new Dictionary<ClrClassObject, List<ContinuationEdge>>(new FastEqualityComparer());

        public void AddVertex(IClrCompositeObject obj)
        {
            var maybeTask = obj as ClrClassObject;
            if (maybeTask?.IsOfTaskType() == true)
            {
                taskVertices.Add(maybeTask);
            }
            else
            {
                otherVertices.Add(obj);
            }
        }

        private static void AssertIsTask(ClrClassObject task)
        {
            if (!task.IsOfTaskType()) throw new ArgumentException($"Not a task: {task}");
        }

        public IEnumerable<ClrClassObject> GetRoots()
        {
            return TaskVertices.Where(task => CountIncoming(task) == 0);
        }

        public IEnumerable<ClrClassObject> Incoming(IClrCompositeObject continueWith)
        {
            List<ContinuationEdge> edgeList;
            if (!incomingEdges.TryGetValue(continueWith, out edgeList)) return Enumerable.Empty<ClrClassObject>();
            return edgeList.Select(e => e.WaitingUpon);
        }

        public int CountIncoming(IClrCompositeObject continueWith)
        {
            List<ContinuationEdge> edgeList;
            if (!incomingEdges.TryGetValue(continueWith, out edgeList)) return 0;
            return edgeList.Count;
        }

        public int CountOutgoing(ClrClassObject task)
        {
            List<ContinuationEdge> edgeList;
            if (!outgoingEdges.TryGetValue(task, out edgeList)) return 0;
            return edgeList.Count;
        }

        public IEnumerable<IClrCompositeObject> Outgoing(ClrClassObject task)
        {
            List<ContinuationEdge> edgeList;
            if (!outgoingEdges.TryGetValue(task, out edgeList)) return Enumerable.Empty<IClrCompositeObject>();
            return edgeList.Select(e => e.ContinueWith);
        }

        public void AddContinuation(ClrClassObject task, IClrCompositeObject continueWith)
        {
            var edge = new ContinuationEdge(task, continueWith);
            taskVertices.Add(task);
            AddVertex(continueWith);
            AddEdge(outgoingEdges, task, edge);
            AddEdge(incomingEdges, continueWith, edge);
        }

        private static void AddEdge<TKey>(Dictionary<TKey, List<ContinuationEdge>> edges, TKey key, ContinuationEdge edge)
        {
            List<ContinuationEdge> edgeList;
            if (!edges.TryGetValue(key, out edgeList))
            {
                edgeList = new List<ContinuationEdge>();
                edges.Add(key, edgeList);
            }
            edgeList.Add(edge);
        }

        public IEnumerable<ClrClassObject> TaskVertices => taskVertices.AsEnumerable();
        public IEnumerable<IClrCompositeObject> AllVertices => taskVertices.Concat(otherVertices).AsEnumerable();

        public struct ContinuationEdge
        {
            public ContinuationEdge(ClrClassObject task, IClrCompositeObject continueWith)
            {
                AssertIsTask(task);
                WaitingUpon = task;
                ContinueWith = continueWith;
            }

            public ClrClassObject WaitingUpon { get; }
            public IClrCompositeObject ContinueWith { get; }
        }

        struct FastEqualityComparer : IEqualityComparer<ContinuationEdge>, IEqualityComparer<IClrCompositeObject>
        {
            public bool Equals(ContinuationEdge x, ContinuationEdge y)
            {
                return Equals(x.WaitingUpon, y.WaitingUpon) && Equals(x.ContinueWith, y.ContinueWith);
            }

            public int GetHashCode(ContinuationEdge obj)
            {
                unchecked { return (GetHashCode(obj.WaitingUpon) * 397) ^ GetHashCode(obj.ContinueWith); }
            }

            public bool Equals(IClrCompositeObject x, IClrCompositeObject y)
            {
                return x.Address.Equals(y.Address) && x.Address.Equals(y.Address);
            }

            public int GetHashCode(IClrCompositeObject obj)
            {
                unchecked { return (obj.Address.GetHashCode() * 397) ^ obj.Address.GetHashCode(); }
            }
        }
    }
}

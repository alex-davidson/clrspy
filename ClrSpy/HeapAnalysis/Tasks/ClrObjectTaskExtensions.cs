using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.HeapAnalysis.Tasks
{
    public static class ClrObjectTaskExtensions
    {
        public static IEnumerable<ClrClassObject> OfTaskType(this IEnumerable<IClrObject> instances)
        {
            return instances.OfType<ClrClassObject>().Where(o => o.Type.CanBeAssignedTo<Task>());
        }
    }
}

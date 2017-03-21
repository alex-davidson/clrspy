using System.Reflection;
using System.Text.RegularExpressions;
using ClrSpy.HeapAnalysis.Model;

namespace ClrSpy.UnitTests.HeapAnalysis.Tasks
{
    public class ContinuationTypeMatcher
    {
        private readonly Regex matcher;

        public ContinuationTypeMatcher(MethodInfo method)
        {
            var methodName = method.Name;
            var className = method.DeclaringType?.Name;
            matcher = new Regex($"{Regex.Escape(className ?? "")}.*<{Regex.Escape(methodName)}>");
        }

        public bool IsMatch(IClrCompositeObject continuation)
        {
            return matcher.IsMatch(continuation.Type.Name);
        }
    }
}

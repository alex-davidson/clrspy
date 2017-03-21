using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ClrSpy.UnitTests.Utils;

namespace ClrSpy.UnitTests.HeapAnalysis.Tasks
{
    class AsyncTaskTargetProcess : ProcessWrapper<AsyncTaskTarget.Program>
    {
        private readonly string testType;

        public AsyncTaskTargetProcess(string testType = null)
        {
            this.testType = testType;
        }

        protected override IEnumerable<string> GetArguments()
        {
            if (testType != null) yield return testType;
        }

        public async Task WaitForTask()
        {
            await Process.StandardOutput.ReadLineAsync();
        }

        public MethodInfo GetMethodInfo(string methodName)
        {
            return typeof(AsyncTaskTarget.Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        protected override void TryShutdown()
        {
            Process.StandardInput.WriteLine();
            Process.WaitForExit();
        }
    }
}

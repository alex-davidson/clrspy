using System.Threading.Tasks;
using ClrSpy.UnitTests.Utils;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    class HeapAnalysisTargetProcess : ProcessWrapper<HeapAnalysisTarget.Program>
    {
        public async Task WaitForTask()
        {
            await Process.StandardOutput.ReadLineAsync();
        }

        protected override void TryShutdown()
        {
            Process.StandardInput.WriteLine();
            Process.WaitForExit();
        }
    }
}

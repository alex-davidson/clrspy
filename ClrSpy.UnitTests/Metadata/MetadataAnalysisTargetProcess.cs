using System.Threading.Tasks;
using ClrSpy.UnitTests.Utils;

namespace ClrSpy.UnitTests.Metadata
{
    class MetadataAnalysisTargetProcess : ProcessWrapper<MetadataAnalysisTarget.Program>
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

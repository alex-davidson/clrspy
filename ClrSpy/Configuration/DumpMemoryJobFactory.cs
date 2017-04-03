using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Debugger;
using ClrSpy.Jobs;

namespace ClrSpy.Configuration
{
    public class DumpMemoryJobFactory : IDebugJobFactory
    {
        public RunningProcessArguments RunningProcess { get; } = new RunningProcessArguments();
        public string DumpFile { get; private set; }
        public bool OverwriteDumpFileIfExists { get; private set; }

        public string GetDumpFilePath() => JobFactoryHelpers.GetDumpFilePath(DumpFile ?? $"memorydump-{RunningProcess.Pid}");

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.AddCollector(RunningProcess);
            options.Add("d=|dumpfile=", "Dump process memory to this path.", o => DumpFile = o);
            options.Add("f|force", "Overwrite the dumpfile if it already exists.", o => OverwriteDumpFileIfExists = true);
        }

        public void Validate()
        {
            if(!RunningProcess.SuspendProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to dump the contents of memory.") { ShowUsage = true };
            JobFactoryHelpers.ValidateDumpFilePathForOutput(GetDumpFilePath(), OverwriteDumpFileIfExists);
        }

        public IDebugJob CreateJob(ConsoleLog console)
        {
            var process = JobFactoryHelpers.TryResolveTargetProcess(RunningProcess, console);
            var debugTarget = new DebugRunningProcess(process, DebugMode.Snapshot);
            var fullDumpFilePath = JobFactoryHelpers.ValidateDumpFilePathForOutput(GetDumpFilePath(), OverwriteDumpFileIfExists);
            return new DumpMemoryJob(debugTarget, fullDumpFilePath) { OverwriteDumpFileIfExists = OverwriteDumpFileIfExists };
        }
    }
}

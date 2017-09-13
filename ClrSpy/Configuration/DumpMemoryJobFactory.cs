using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Debugger;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class DumpMemoryJobFactory : IDebugJobFactory
    {
        public RunningProcessArguments RunningProcess { get; } = new RunningProcessArguments();
        public string DumpFile { get; private set; }
        public bool OverwriteDumpFileIfExists { get; private set; }

        public string GetDumpFilePath(IProcessInfo process) => JobFactoryHelpers.GetDumpFilePath(DumpFile ?? $"memorydump-{process.Pid}");

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.AddCollector(RunningProcess);
            options.Add("d=|dumpfile=", "Dump process memory to this path.", o => DumpFile = o);
            options.Add("f|force", "Overwrite the dumpfile if it already exists.", o => OverwriteDumpFileIfExists = true);
        }

        public void Validate()
        {
            if(!RunningProcess.SuspendProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to dump the contents of memory.") { ShowUsage = true };
            IProcessInfo process;
            if (JobFactoryHelpers.TryResolveTargetProcessQuietly(RunningProcess, out process))
            {
                JobFactoryHelpers.ValidateDumpFilePathForOutput(GetDumpFilePath(process), OverwriteDumpFileIfExists);
            }
        }

        public IDebugJob CreateJob(ConsoleLog console)
        {
            var process = JobFactoryHelpers.TryResolveTargetProcess(RunningProcess, console);
            var debugTarget = new DebugRunningProcess(process, DebugMode.Snapshot);
            var fullDumpFilePath = JobFactoryHelpers.ValidateDumpFilePathForOutput(GetDumpFilePath(process), OverwriteDumpFileIfExists);
            return new DumpMemoryJob(debugTarget, fullDumpFilePath) { OverwriteDumpFileIfExists = OverwriteDumpFileIfExists };
        }
    }
}

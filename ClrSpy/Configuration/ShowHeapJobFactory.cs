using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Debugger;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class ShowHeapJobFactory : IDebugJobFactory
    {
        public RunningProcessArguments RunningProcess { get; } = new RunningProcessArguments();
        public DumpedProcessArguments DumpedProcess { get; } = new DumpedProcessArguments();

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            RunningProcess.ReceiveFrom(options);
            DumpedProcess.ReceiveFrom(options);
        }

        public void Validate()
        {
            if (RunningProcess.WasSpecified)
            {
                if(!RunningProcess.SuspendProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to show heap information.") { ShowUsage = true };
            }
        }

        public IDebugJob CreateJob(ConsoleLog console)
        {
            var debugTarget = GetDebugTarget(console);
            return new ShowHeapJob(debugTarget);
        }

        private IDebugSessionTarget GetDebugTarget(ConsoleLog console)
        {
            if (RunningProcess.WasSpecified)
            {
                var process = JobFactoryHelpers.TryResolveTargetProcess(RunningProcess, console);
                return new DebugRunningProcess(process, RunningProcess.SuspendProcess ? DebugMode.Snapshot : DebugMode.Observe);
            }
            if (DumpedProcess.WasSpecified)
            {
                var fullDumpFilePath = JobFactoryHelpers.ValidateDumpFilePathForInput(JobFactoryHelpers.GetDumpFilePath(DumpedProcess.DumpFile));
                return new DebugMemoryDump(fullDumpFilePath);
            }
            throw new ErrorWithExitCodeException(1, new ProcessNotSpecifiedException().Message) { ShowUsage = true };
        }
    }
}

using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Debugger;
using ClrSpy.Jobs;

namespace ClrSpy.Configuration
{
    public class ShowStacksJobFactory : IDebugJobFactory
    {
        public RunningProcessArguments RunningProcess { get; } = new RunningProcessArguments();

        void IReceiveOptions.ReceiveFrom(OptionSet options) => RunningProcess.ReceiveFrom(options);

        public void Validate()
        {
        }

        public IDebugJob CreateJob(ConsoleLog console)
        {
            var process = JobFactoryHelpers.TryResolveTargetProcess(RunningProcess, console);
            var debugTarget = new DebugRunningProcess(process, RunningProcess.SuspendProcess ? DebugMode.Snapshot : DebugMode.Observe);
            return new ShowStacksJob(debugTarget);
        }
    }
}

using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;

namespace ClrSpy.Configuration
{
    public class ShowHeapJobFactory : IDebugJobFactory
    {
        public RunningProcessArguments RunningProcess { get; } = new RunningProcessArguments();

        void IReceiveOptions.ReceiveFrom(Options options) => options.AddCollector(RunningProcess);

        public void Validate()
        {
            if(!RunningProcess.SuspendProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to show heap information.") { ShowUsage = true };
        }

        public IDebugJob CreateJob(ConsoleLog console)
        {
            var process = JobFactoryHelpers.TryResolveTargetProcess(RunningProcess, console);
            return new ShowHeapJob(process);
        }
    }
}

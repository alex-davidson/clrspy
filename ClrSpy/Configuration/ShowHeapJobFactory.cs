using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class ShowHeapJobFactory : IDebugJobFactory
    {
        public void AddOptionDefinitions(Options options)
        {
        }

        public IDebugJobFactory Configure(ref string[] jobSpecificArgs, bool activelyAttachToProcess)
        {
            if(!activelyAttachToProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to show heap information.") { ShowUsage = true };
            return this;
        }

        public IDebugJob CreateJob(IProcessInfo process)
        {
            return new ShowHeapJob(process);
        }
    }
}
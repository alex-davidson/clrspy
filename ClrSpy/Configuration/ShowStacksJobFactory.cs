using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class ShowStacksJobFactory : IDebugJobFactory
    {
        public bool ActivelyAttachToProcess { get; private set; }

        public void AddOptionDefinitions(Options options)
        {
        }

        public IDebugJobFactory Configure(ref string[] jobSpecificArgs, bool activelyAttachToProcess)
        {
            return new ShowStacksJobFactory { ActivelyAttachToProcess = activelyAttachToProcess };
        }

        public IDebugJob CreateJob(IProcessInfo process)
        {
            return new ShowStacksJob(process, ActivelyAttachToProcess);
        }
    }
}
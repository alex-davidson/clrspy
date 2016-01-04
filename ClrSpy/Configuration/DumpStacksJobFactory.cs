using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class DumpStacksJobFactory : IDebugJobFactory
    {
        public bool ActivelyAttachToProcess { get; private set; }

        public void AddOptionDefinitions(Options options)
        {
        }

        public IDebugJobFactory Configure(ref string[] jobSpecificArgs, bool activelyAttachToProcess)
        {
            return new DumpStacksJobFactory { ActivelyAttachToProcess = activelyAttachToProcess };
        }

        public IDebugJob CreateJob(IProcessInfo process)
        {
            return new DumpStacksJob(process, ActivelyAttachToProcess);
        }
    }
}
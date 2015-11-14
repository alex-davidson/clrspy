using ClrSpy.CliSupport;

namespace ClrSpy.Jobs
{
    public class DebugJobFactory
    {
        public IDebugJob Create(Arguments arguments)
        {
            if(arguments.Pid == null) throw new ErrorWithExitCodeException(1, "No process ID specified.") { ShowUsage = true };
            switch(arguments.JobType)
            {
                case JobType.DumpStacks:
                    {
                        return new DumpStacksJob(arguments.Pid.Value, arguments.PauseTargetProcess);
                    }

                case JobType.DumpHeap:
                    {
                        if(!arguments.PauseTargetProcess) throw new ErrorWithExitCodeException(1, "The -x switch is required in order to dump heap information.") { ShowUsage = true };
                        return new DumpHeapJob(arguments.Pid.Value, arguments.PauseTargetProcess);
                    }
                    
                default:
                    throw new ErrorWithExitCodeException(1, $"Unsupported operation: {arguments.JobType}");
            }
        }
    }
}

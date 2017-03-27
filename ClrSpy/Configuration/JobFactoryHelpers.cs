using System.Linq;
using ClrSpy.CliSupport;
using ClrSpy.Processes;
using x86Thunk;

namespace ClrSpy.Configuration
{
    public static class JobFactoryHelpers
    {
        public static IProcessInfo TryResolveTargetProcess(RunningProcessArguments identifiers, ConsoleLog console)
        {
            var processResolver = new ProcessResolver(new ProcessFinder());
            try
            {
                return processResolver.ResolveTargetProcess(identifiers.Pid, identifiers.Name);
            }
            catch(ProcessNotSpecifiedException ex)
            {
                throw new ErrorWithExitCodeException(1, ex.Message) { ShowUsage = true };
            }
            catch(ProcessNotFoundException ex) when(ex.Candidates.Any())
            {
                new ProcessListDescriber().DescribeCandidateProcesses(ex.Candidates.ToList(), console);
                if(Bootstrap.WasUsed) throw ErrorWithExitCodeException.Propagate(3);
                throw new ErrorWithExitCodeException(3, "Please specify a unique process Id using the -p switch.");
            }
            catch(ProcessNotFoundException ex)
            {
                throw new ErrorWithExitCodeException(3, ex.Message);
            }
        }
    }
}

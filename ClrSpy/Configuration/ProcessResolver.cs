using System;
using System.Linq;
using ClrSpy.CliSupport;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public class ProcessResolver
    {
        private readonly IProcessFinder processFinder;

        public ProcessResolver(IProcessFinder processFinder)
        {
            this.processFinder = processFinder;
        }
        
        public IProcessInfo ResolveTargetProcess(int? pid, string processName)
        {
            if(pid == null)
            {
                if(String.IsNullOrWhiteSpace(processName)) throw new ProcessNotSpecifiedException();

                var candidates = processFinder.FindProcessesByName(processName);
                if(!candidates.Any()) throw new ProcessNotFoundException($"No process with name '{processName}' is running");
                if(candidates.Count() == 1) return candidates.Single();

                throw new ProcessNotFoundException(candidates);
            }
            
            if(!String.IsNullOrWhiteSpace(processName))
            {
                return processFinder.VerifyProcessName(processName, pid.Value);
            }
            return processFinder.GetProcessById(pid.Value);
        }
    }
}

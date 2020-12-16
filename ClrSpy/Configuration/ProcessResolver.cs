using System;
using System.Linq;
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
        
        public IProcessInfo ResolveTargetProcess(int? pid, string processName, string appPoolNamePrefix)
        {
            if(!String.IsNullOrWhiteSpace(appPoolNamePrefix) && !String.IsNullOrWhiteSpace(processName))
            {
                throw new InvalidProcessSpecificationException();
            }

            if(pid == null)
            {
                if(!String.IsNullOrWhiteSpace(appPoolNamePrefix))
                {
                    return ResolveSingleProcessByAppPoolNamePrefix(appPoolNamePrefix);
                }
                if(!String.IsNullOrWhiteSpace(processName))
                {
                    return ResolveSingleProcessByName(processName);
                }
                throw new ProcessNotSpecifiedException();
            }

            if(!String.IsNullOrWhiteSpace(appPoolNamePrefix))
            {
                return processFinder.VerifyAppPoolNamePrefix(appPoolNamePrefix, pid.Value);
            }
            if(!String.IsNullOrWhiteSpace(processName))
            {
                return processFinder.VerifyProcessName(processName, pid.Value);
            }
            return processFinder.GetProcessById(pid.Value);
        }

        private IProcessInfo ResolveSingleProcessByName(string processName)
        {
            var candidates = processFinder.FindProcessesByName(processName);
            if(!candidates.Any()) throw new ProcessNotFoundException($"No process with name '{processName}' is running");
            if(candidates.Length == 1) return candidates.Single();

            throw new ProcessNotFoundException(candidates);
        }

        private IProcessInfo ResolveSingleProcessByAppPoolNamePrefix(string appPoolNamePrefix)
        {
            var candidates = processFinder.FindProcessesByAppPoolNamePrefix(appPoolNamePrefix);
            if(!candidates.Any()) throw new ProcessNotFoundException($"No process for application pool name starting with '{appPoolNamePrefix}' is running");
            if(candidates.Length == 1) return candidates.Single();

            throw new ProcessNotFoundException(candidates);
        }
    }
}

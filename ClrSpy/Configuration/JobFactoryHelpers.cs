using System;
using System.IO;
using System.Linq;
using ClrSpy.CliSupport;
using ClrSpy.Debugger;
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

        public static bool TryResolveTargetProcessQuietly(RunningProcessArguments identifiers, out IProcessInfo process)
        {
            var processResolver = new ProcessResolver(new ProcessFinder());
            try
            {
                process = processResolver.ResolveTargetProcess(identifiers.Pid, identifiers.Name);
                return true;
            }
            catch
            {
                process = null;
                return false;
            }
        }

        public static string GetDumpFilePath(string dumpFileSpecification) => Path.Combine(Environment.CurrentDirectory, dumpFileSpecification);

        public static string ValidateDumpFilePathForOutput(string rawDumpFilePath, bool overwrite)
        {
            var dumpFilePath = Path.GetFullPath(rawDumpFilePath);
            if (Directory.Exists(dumpFilePath))
            {
                throw new ErrorWithExitCodeException(1, $"The specified path is a directory: {dumpFilePath}");
            }
            if (!overwrite && File.Exists(dumpFilePath))
            {
                throw new ErrorWithExitCodeException(1, $"The specified file exists and --force was not specified: {dumpFilePath}");
            }
            return dumpFilePath;
        }

        public static string ValidateDumpFilePathForInput(string rawDumpFilePath)
        {
            var dumpFilePath = Path.GetFullPath(rawDumpFilePath);
            if (Directory.Exists(dumpFilePath))
            {
                throw new ErrorWithExitCodeException(1, $"The specified path is a directory: {dumpFilePath}");
            }
            if (!File.Exists(dumpFilePath))
            {
                throw new ErrorWithExitCodeException(1, $"The specified file does not exist: {dumpFilePath}");
            }
            return dumpFilePath;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Configuration;
using ClrSpy.Debugger;
using ClrSpy.Jobs;
using ClrSpy.Processes;
using Microsoft.Win32;
using x86Thunk;

namespace ClrSpy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (!AssertSufficientDotNetVersion()) return 255;
            
            var arguments = new Arguments();
            var options = CreateOptions(arguments);
            try
            {
                var remainingArgs = options.Parse(args).ToArray();
                arguments.ParseRemaining(ref remainingArgs);

                var console = new ConsoleLog(Console.Error, arguments.Verbose);
                var jobFactory = SelectFactory(arguments.JobType ?? JobType.DumpStacks);
                if (jobFactory == null) throw new ErrorWithExitCodeException(1, $"Unsupported operation: {arguments.JobType}") { ShowUsage = true };

                var configuredFactory = jobFactory.Configure(ref remainingArgs, arguments.ActivelyAttachToProcess);

                var process = ResolveTargetProcess(arguments, console);
                var job = configuredFactory.CreateJob(process);

                console.WriteLineVerbose($"Running as a {ProcessArchitecture.FromCurrentProcess().Describe()} process.");
                ExecuteJob(console, job);

                return 0;
            }
            catch (Requires32BitEnvironmentException)
            {
                return Bootstrap.RecurseInto32BitProcess();
            }
            catch (Requires64BitEnvironmentException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 64;
            }
            catch(ErrorWithExitCodeException ex)
            {
                if(!String.IsNullOrEmpty(ex.Message))
                {
                    Console.Error.WriteLine(ex.Message);
                }
                if(ex.ShowUsage)
                {
                    ShowUsage(arguments.JobType, options);
                }
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                // Otherwise-unhandled exception.
                Console.Error.WriteLine(ex);
                return 255;
            }
        }

        private static void ExecuteJob(ConsoleLog console, IDebugJob job)
        {
            try
            {
                job.Run(Console.Out, console);
            }
            catch (NoClrModulesFoundException ex)
            {
                throw new ErrorWithExitCodeException(2, ex);
            }
        }

        private static IProcessInfo ResolveTargetProcess(Arguments arguments, ConsoleLog console)
        {
            var processResolver = new ProcessResolver(new ProcessFinder());
            try
            {
                return processResolver.ResolveTargetProcess(arguments.Pid, arguments.ProcessName);
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
        
        public static IDebugJobFactory SelectFactory(JobType jobType)
        {
            switch(jobType)
            {
                case JobType.DumpStacks:
                    return new DumpStacksJobFactory();

                case JobType.DumpHeap:
                    return new DumpHeapJobFactory();
                    
                case JobType.TraceExceptions:
                    return new TraceExceptionsJobFactory();

                default:
                    throw new ErrorWithExitCodeException(1, $"Unsupported operation: {jobType}");
            }
        }

        private static void ShowUsage(JobType? jobType, Options options)
        {
            var jobFactory = jobType == null ? null : SelectFactory(jobType.Value);
            var codeBase = Bootstrap.GetEntryAssemblyUri().LocalPath;
            if (jobFactory == null)
            {
                Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} <mode> [options]");
                Console.Error.WriteLine("  where mode is one of: dumpstacks, dumpheap");
            }
            else
            {
                var convenientJobTypeString = jobType.ToString().ToLower();
                Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} {convenientJobTypeString} [options]");
                jobFactory.AddOptionDefinitions(options);
            }
            options.WriteOptionDescriptions(Console.Error);
        }

        private static bool AssertSufficientDotNetVersion()
        {
            var dotNetVersion = GetDotNetVersion();
            if (dotNetVersion > new Version(4, 5)) return true;

            Console.Error.WriteLine("This application must run under .NET 4.5 or later.");
            if (dotNetVersion != null)
            {
                Console.Error.WriteLine($"Currently running under: {dotNetVersion}");
            }
            else
            {
                Console.Error.WriteLine("The current .NET version could not be determined.");
            }
            return false;
        }

        private static Version GetDotNetVersion()
        {
            if(Environment.Version.Major < 4) return Environment.Version;
            try
            {
                var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client");
                if(versionKey == null) return null; // Should be there for .NET 4.0+, surely?
                var versionString = versionKey.GetValue("Version")?.ToString();

                Version version;
                if(!Version.TryParse(versionString, out version)) return null;
                return version;
            }
            catch
            {
                return null;
            }
        }

        public static Options CreateOptions(Arguments arguments)
        {
            return new Options {
                { "x|exclusive", "Attach to the target process while reading its state, instead of passively observing it. Required for obtaining heap information.", o => arguments.ActivelyAttachToProcess = true },
                { "v|verbose", "Increase logging verbosity.", o => arguments.Verbose = true },
                { "p=|pid=|process-id=", "PID of the target process.", (int o) => arguments.Pid = o },
                { "n=|name=|process-name=", "Name of the target process.", o => arguments.ProcessName = o },
            };
        }
    }
}

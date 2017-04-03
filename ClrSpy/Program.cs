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
using Microsoft.Win32;
using x86Thunk;

namespace ClrSpy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (!AssertSufficientDotNetVersion()) return 255;

            var options = new OptionSet();
            var mainArguments = options.AddCollector(new Arguments());
            try
            {
                var jobFactory = ParseArguments(options, mainArguments, args);
                var console = new ConsoleLog(Console.Error, mainArguments.Verbose);

                jobFactory.Validate();

                var job = jobFactory.CreateJob(console);

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
                    ShowUsage(mainArguments.JobType, options);
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

        public static IDebugJobFactory ParseArguments(OptionSet options, Arguments mainArguments, string[] args)
        {
            mainArguments.JobType = new JobTypeParser().ParseJobTypeInPlace(ref args);
            if (mainArguments.JobType == null) throw new ErrorWithExitCodeException(1, "") { ShowUsage = true };
            var jobFactory = SelectFactory(mainArguments.JobType.Value);
            if (jobFactory == null) throw new ErrorWithExitCodeException(1, $"Unsupported operation: {mainArguments.JobType}") {ShowUsage = true};

            options.AddCollector(jobFactory);

            var remainingArgs = options.Parse(args).ToArray();
            if (mainArguments.ShowUsage)
            {
                throw new ErrorWithExitCodeException(1, "") { ShowUsage = true };
            }
            if (remainingArgs.Any())
            {
                throw new ErrorWithExitCodeException(1, $"Unrecognised arguments: {String.Join(" ", remainingArgs)}");
            }
            return jobFactory;
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
        
        public static IDebugJobFactory SelectFactory(JobType jobType)
        {
            switch(jobType)
            {
                case JobType.ShowStacks:
                    return new ShowStacksJobFactory();

                case JobType.ShowHeap:
                    return new ShowHeapJobFactory();

                case JobType.DumpMemory:
                    return new DumpMemoryJobFactory();
                    
                default:
                    throw new ErrorWithExitCodeException(1, $"Unsupported operation: {jobType}");
            }
        }

        private static void ShowUsage(JobType? jobType, OptionSet options)
        {
            var jobFactory = jobType == null ? null : SelectFactory(jobType.Value);
            var codeBase = Bootstrap.GetEntryAssemblyUri().LocalPath;
            if (jobFactory == null)
            {
                Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} <mode> [options]");
                Console.Error.WriteLine("  where mode is one of: showstacks, showheap, dumpmemory");
            }
            else
            {
                var convenientJobTypeString = jobType.ToString().ToLower();
                Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} {convenientJobTypeString} [options]");
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

        public class Arguments : IReceiveOptions
        {
            public bool Verbose { get; set; }
            public bool ShowUsage { get; set; }
            public JobType? JobType { get; set; }

            public void ReceiveFrom(OptionSet options)
            {
                options.Add("v|verbose", "Increase logging verbosity.", o => Verbose = true);
                options.Add("h|?|help", "Show usage information.", o => ShowUsage = true);
            }
        }
    }
}

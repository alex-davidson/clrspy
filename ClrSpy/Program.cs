using Microsoft.Diagnostics.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Configuration;
using ClrSpy.Jobs;
using ClrSpy.Processes;
using Microsoft.Win32;

namespace ClrSpy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (!AssertSufficientDotNetVersion()) return 255;
            
            var configurer = new DebugJobConfigurer(new ProcessFinder());
            var arguments = new Arguments();
            var options = CreateOptions(arguments);
            try
            {
                var remaining = options.Parse(args).ToArray();
                arguments.ParseRemaining(ref remaining);
                
                var console = new ConsoleLog(Console.Error, arguments.Verbose);
                string[] remainingArgs = remaining;
                var jobFactory = configurer.SelectFactory(arguments.JobType ?? JobType.DumpStacks);
                var configuredFactory = jobFactory.Configure(ref remainingArgs, arguments.ActivelyAttachToProcess);

                var process = configurer.ResolveTargetProcess(arguments, console);
                var job = configuredFactory.CreateJob(process);

                console.WriteLineVerbose($"Running as a {ProcessArchitecture.FromCurrentProcess().Describe()} process.");
                job.Run(Console.Out, console);

                return 0;
            }
            catch(Requires32BitEnvironmentException ex)
            {
                return ex.ExecuteIn32BitProcess(GetPathToSelf(), args);
            }
            catch(ErrorWithExitCodeException ex)
            {
                Console.Error.WriteLine(ex.Message);
                if(ex.ShowUsage)
                {
                    var codeBase = GetPathToSelf();
                    Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} <mode> [options]");
                    Console.Error.WriteLine("  where mode is one of: dumpstacks, dumpheap");
                    options.WriteOptionDescriptions(Console.Error);
                }
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 255;
            }
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
        
        private static string GetPathToSelf()
        {
            return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }
    }
}

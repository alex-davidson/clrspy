using Microsoft.Diagnostics.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;

namespace ClrSpy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var arguments = new Arguments();
            var options = CreateOptions(arguments);
            try
            {
                var remaining = options.Parse(args).ToArray();
                arguments.ParseRemaining(remaining);
                
                var job = new DebugJobFactory().Create(arguments);
                var console = new ConsoleLog(Console.Error, arguments.Verbose);
                console.WriteLineVerbose(Environment.Is64BitProcess ? "Running as 64-bit process." : "Running as 32-bit process.");
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

        public static Options CreateOptions(Arguments arguments)
        {
            return new Options {
                { "x|exclusive", "Pause the target process while reading its state. Required for obtaining heap information.", o => arguments.PauseTargetProcess = true },
                { "v|verbose", "Increase logging verbosity.", o => arguments.Verbose = true },
                { "p=|pid=|process-id=", "PID of the target process.", (int o) => arguments.Pid = o },
            };
        }
        
        private static string GetPathToSelf()
        {
            return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }
    }
}

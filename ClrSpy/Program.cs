using Microsoft.Diagnostics.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;

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
                Run(arguments);
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
                    Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} <pid> [options]");
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
                { "x|exclusive", "Pause the target process while reading its state.", o => arguments.PauseTargetProcess = true },
                { "v|verbose", "Increase logging verbosity.", o => arguments.Verbose = true },
                { "p=|pid=|process-id=", "PID of the target process.", (int o) => arguments.Pid = o },
            };
        }
        
        private static string GetPathToSelf()
        {
            return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }
            
        private static void Run(Arguments arguments)
        {
            switch(arguments.JobType)
            {
                case JobType.DumpStacks:
                    var job = new DumpStacksJob(arguments.Pid.Value, arguments.PauseTargetProcess);
                    var console = new ConsoleLog(Console.Error, arguments.Verbose);

                    console.WriteLineVerbose(Environment.Is64BitProcess ? "Running as 64-bit process." : "Running as 32-bit process.");
                    job.Run(Console.Out, console);
                    break;

                default:
                    throw new ErrorWithExitCodeException(1, $"Unsupported operation: {arguments.JobType}");
            }
        }
        
    }
}
